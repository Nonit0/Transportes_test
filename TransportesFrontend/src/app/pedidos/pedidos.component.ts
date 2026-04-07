import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { environment } from '../../environments/environment';

export interface Pedido {
  id: string;
  clienteId: string;
  fechaPedido: string;
  estado: string;
  cliente: any;
  pedidoDetalles: any[];
}

export interface Cliente {
  id: string;
  nombre: string;
}

export interface Producto {
  id: string;
  nombre: string;
  volumenUnitario: number;
  pesoUnitario: number;
  precioUnitario: number;
}

@Component({
  selector: 'app-pedidos',
  templateUrl: './pedidos.component.html',
  styleUrls: ['./pedidos.component.css']
})
export class PedidosComponent implements OnInit {
  pedidos: Pedido[] = [];
  clientes: Cliente[] = [];
  productos: Producto[] = [];

  pedidoForm: FormGroup;
  mostrarFormulario = false;

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private fb: FormBuilder) {
    this.pedidoForm = this.fb.group({
      clienteId: ['', Validators.required],
      fechaPedido: [''], // Opcional, si está vacío el backend pondrá la actual
      pedidoDetalles: this.fb.array([], Validators.required)
    });
  }

  ngOnInit(): void {
    this.cargarPedidos();
    this.cargarClientes();
    this.cargarProductos();
    this.agregarProducto(); // Añade la primera fila por defecto
  }

  // Gets the FormArray easily
  get pedidoDetalles(): FormArray {
    return this.pedidoForm.get('pedidoDetalles') as FormArray;
  }

  agregarProducto() {
    const detalleForm = this.fb.group({
      productoId: ['', Validators.required],
      cantidad: [1, [Validators.required, Validators.min(1)]]
    });
    this.pedidoDetalles.push(detalleForm);
  }

  eliminarProducto(index: number) {
    if (this.pedidoDetalles.length > 1) {
      this.pedidoDetalles.removeAt(index);
    } else {
      alert("⚠️ El pedido debe tener al menos un producto.");
    }
  }

  cargarPedidos() {
    this.http.get<any>(`${this.apiUrl}/Pedidos`)
      .subscribe({
        next: (data) => this.pedidos = data.$values ? data.$values : data,
        error: (err) => console.error('Error al cargar pedidos', err)
      });
  }

  cargarClientes() {
    this.http.get<any>(`${this.apiUrl}/Clientes`)
      .subscribe({
        next: (data) => this.clientes = data.$values ? data.$values : data,
        error: (err) => console.error('Error al cargar clientes', err)
      });
  }

  cargarProductos() {
    this.http.get<any>(`${this.apiUrl}/Productos`)
      .subscribe({
        next: (data) => this.productos = data.$values ? data.$values : data,
        error: (err) => console.error('Error al cargar productos', err)
      });
  }

  guardarPedido() {
    if (this.pedidoForm.invalid) {
      alert("⚠️ Revisa los campos obligatorios del pedido.");
      return;
    }

    const valueOriginal = this.pedidoForm.value;
    
    // Preparar el cuerpo: el payload debe coincidir con los campos de C#
    const payload = {
      clienteId: valueOriginal.clienteId,
      fechaPedido: valueOriginal.fechaPedido || null,
      pedidoDetalles: valueOriginal.pedidoDetalles.map((d: any) => ({
        productoId: d.productoId,
        cantidad: d.cantidad
      }))
    };

    this.http.post<any>(`${this.apiUrl}/Pedidos`, payload)
      .subscribe({
        next: () => {
          this.cargarPedidos();
          this.resetearFormulario();
          this.mostrarFormulario = false;
        },
        error: (err) => {
          const msg = err.error?.mensaje || 'Error al crear el pedido.';
          alert('⚠️ ' + msg);
        }
      });
  }

  cambiarEstado(pedidoId: string, nuevoEstado: string) {
    // Para simplificar, le mandamos comillas al cuerpo JSON si es un string directo en .net 5
    // O si definimos en c# [FromBody] string nuevoEstado, Angular puede usar JSON.stringify.
    this.http.put(`${this.apiUrl}/Pedidos/${pedidoId}/estado`, `"${nuevoEstado}"`, {
      headers: { 'Content-Type': 'application/json' }
    }).subscribe({
        next: () => {
           // Actualiza localmente sin recargar si prefieres
           const p = this.pedidos.find(x => x.id === pedidoId);
           if (p) p.estado = nuevoEstado;
        },
        error: () => alert('Hubo un problema al cambiar el estado.')
    });
  }

  eliminarPedido(id: string) {
    if (confirm('¿Estás seguro de que deseas eliminar (soft delete) este pedido?')) {
      this.http.delete(`${this.apiUrl}/Pedidos/${id}`)
        .subscribe({
          next: () => this.pedidos = this.pedidos.filter(c => c.id !== id),
          error: () => alert('Hubo un problema al eliminar el pedido.')
        });
    }
  }

  resetearFormulario() {
    this.pedidoForm.reset();
    this.pedidoDetalles.clear();
    this.agregarProducto(); // Pone al menos uno visible
  }

  getNombreProducto(id: string): string {
    return this.productos.find(p => p.id === id)?.nombre || 'Desconocido';
  }
}
