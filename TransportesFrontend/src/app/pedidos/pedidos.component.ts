import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { ClarityIcons, trashIcon, plusIcon, banIcon, inboxIcon } from '@cds/core/icon';

ClarityIcons.addIcons(trashIcon, plusIcon, banIcon, inboxIcon);

@Component({
  selector: 'app-pedidos',
  templateUrl: './pedidos.component.html',
  styleUrls: ['./pedidos.component.css']
})
export class PedidosComponent implements OnInit {
  pedidos: any[] = []; // Más adelante lo tiparemos con una interfaz Pedido
  clientes: any[] = [];   // Lista para el desplegable
  productos: any[] = [];  // Lista para el desplegable

  cargando = true;
  apiUrl = environment.apiUrl;

  // ==========================================
  // VARIABLES DEL MODAL DE NUEVO PEDIDO
  // ==========================================
  mostrarModal = false;
  nuevoPedido = {   
    clienteId: '',
    estado: 'Pendiente',
    pedidoDetalles: [
      { pedidoId: '0', productoId: '', cantidad: 1 } // Empieza siempre con una línea vacía
    ]
  };

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargarPedidos();
    this.cargarClientes();
    this.cargarProductos();
  }

  // ==========================================
  // CARGA DE DATOS (GET)
  // ==========================================
  cargarPedidos() {
    this.cargando = true;
    this.http.get<any>(`${this.apiUrl}/Pedidos`).subscribe({
      next: (data) => {
        // Desempaquetamos el $values de .NET si existe
        this.pedidos = data.$values ? data.$values : data;
        this.cargando = false;
      },
      error: (err) => {
        console.error('Error al cargar pedidos', err);
        this.cargando = false;
      }
    });
  }

  cargarClientes() {
    this.http.get<any>(`${this.apiUrl}/Clientes`).subscribe({
      next: (data) => this.clientes = data.$values ? data.$values : data,
      error: (err) => console.error('Falta endpoint de clientes', err)
    });
  }

  cargarProductos() {
    this.http.get<any>(`${this.apiUrl}/Productos`).subscribe({
      next: (data) => this.productos = data.$values ? data.$values : data,
      error: (err) => console.error('Falta endpoint de productos', err)
    });
  }

  // ==========================================
  // LÓGICA DEL MODAL DINÁMICO (POST)
  // ==========================================
  abrirModal() {
    this.mostrarModal = true;
  }

  cerrarModal() {
    this.mostrarModal = false;
    this.nuevoPedido = { 
      clienteId: '', 
      estado: 'Pendiente',   
      pedidoDetalles: [{ pedidoId: '', productoId: '', cantidad: 1 }] 
    };
  }

  // Añadir una nueva fila de producto al pedido
  agregarLineaDetalle() {
    this.nuevoPedido.pedidoDetalles.push({ pedidoId: '0', productoId: '', cantidad: 1 });
  }

  // Quitar una fila (pasando el índice)
  eliminarLineaDetalle(index: number) {
    if (this.nuevoPedido.pedidoDetalles.length > 1) {
      this.nuevoPedido.pedidoDetalles.splice(index, 1);
    } else {
      alert("El pedido debe tener al menos un producto.");
    }
  }

  guardarPedido() {
    // Validación básica del frontend
    if (!this.nuevoPedido.clienteId) {
      alert("Por favor, selecciona un cliente.");
      return;
    }

    // Comprobar que no hay productos vacíos o cantidades inválidas
    const invalidos = this.nuevoPedido.pedidoDetalles.some(d => !d.productoId || d.cantidad <= 0);
    if (invalidos) {
      alert("Revisa las líneas del pedido. Faltan productos o la cantidad es inválida.");
      return;
    }

    this.http.post<any>(`${this.apiUrl}/Pedidos`, this.nuevoPedido).subscribe({
      next: (creado) => {
        const pedido = creado.$values ? creado.$values : creado;
        this.pedidos.unshift(pedido); // Lo ponemos el primero en la tabla
        this.cerrarModal();
      },
      error: (err) => {
        console.error('Error al crear', err);
        alert(err.error?.mensaje || 'Hubo un problema al guardar el pedido.');
      }
    });
  }

  // ==========================================
  // ACCIONES DE LA TABLA (PUT / DELETE)
  // ==========================================
  cambiarEstado(id: string, nuevoEstado: string) {
    // Para que el PUT funcione con un string simple en el body, a veces 
    // hay que enviarlo como un objeto JSON o entre comillas dobles.
    this.http.put(`${this.apiUrl}/Pedidos/${id}/estado`, `"${nuevoEstado}"`, {
      headers: { 'Content-Type': 'application/json' }
    }).subscribe({
      next: () => {
        // Actualizamos visualmente el estado sin recargar todo
        const pedido = this.pedidos.find(p => p.id === id);
        if (pedido) pedido.estado = nuevoEstado;
      },
      error: (err) => alert('Error al cambiar el estado')
    });
  }

  eliminarPedido(id: string) {
    if (confirm('¿Estás seguro de cancelar y eliminar este pedido?')) {
      this.http.delete(`${this.apiUrl}/Pedidos/${id}`).subscribe({
        next: () => {
          // COMO USAMOS SOFT DELETE: Ya no lo filtramos del array. 
          // Simplemente le ponemos una fecha de borrado local para que la tabla lo pinte en rojo.
          const pedido = this.pedidos.find(p => p.id === id);
          if (pedido) pedido.deletedAt = new Date().toISOString(); 
        },
        error: (err) => alert('Error al eliminar')
      });
    }
  }
}