import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';

export interface Carga {
  id: string;
  camionId: string;
  conductorId: string;
  historicoOrigenNombre: string;
  historicoDestinoNombre: string;
  fechaSalida: string;
  fechaLlegadaEstimada?: string;
  camion?: any;
  conductor?: any;
  cargaPedidos?: any;
}

@Component({
  selector: 'app-cargas',
  templateUrl: './cargas.component.html',
  styleUrls: ['./cargas.component.css']
})
export class CargasComponent implements OnInit {
  private apiUrl = environment.apiUrl;
  cargas: Carga[] = [];
  cargando = true;
  totalItems = 0;

  // Catálogos
  camiones: any[] = [];
  conductores: any[] = [];
  almacenes: any[] = [];
  clientes: any[] = [];
  pedidos: any[] = []; // Solo los "Pendientes"

  isAdmin: boolean = false;

  // Formulario
  formulario = {
    camionId: '',
    conductorId: '',
    origenAlmacenId: '',
    tipoDestino: 'cliente', // 'almacen' o 'cliente'
    destinoAlmacenId: '',
    destinoClienteId: '',
    cargaPedidos: [] as { pedidoId: string }[]
  };

  creandoModal = false;

  // State calculations
  pesoLlenado = 0;
  volumenLlenado = 0;
  camionSeleccionado: any = null;

  constructor(private http: HttpClient, private authService: AuthService) {
    this.isAdmin = this.authService.getRol() === 'Administrador' || this.authService.getRol() === 'Admin';
  }

  ngOnInit(): void {
    // datagrid refresh will call cargarCargas automatically on init with state
  }

  refresh(state: any) {
    this.cargando = true;
    const page = state.page ? state.page.current : 1;
    const limit = state.page ? state.page.size : 50;
    this.cargarCargas(page, limit);
  }

  cargarCargas(page: number = 1, limit: number = 50) {
    this.http.get<any>(`${this.apiUrl}/Cargas?page=${page}&limit=${limit}`)
      .subscribe({
        next: (res) => {
          const responseData = res.data ?? res;
          this.cargas = responseData.$values ? responseData.$values : responseData;
          this.totalItems = res.totalItems !== undefined ? res.totalItems : this.cargas.length;
          this.cargando = false;
        },
        error: (err) => {
          console.error(err);
          this.cargando = false;
        }
      });
  }

  abrirGenerador() {
    this.creandoModal = true;
    this.formulario = {
      camionId: '', conductorId: '', origenAlmacenId: '', tipoDestino: 'cliente',
      destinoAlmacenId: '', destinoClienteId: '', cargaPedidos: []
    };
    this.pesoLlenado = 0;
    this.volumenLlenado = 0;
    this.camionSeleccionado = null;
    this.cargarCatalogos();
  }

  cerrarGenerador() {
    this.creandoModal = false;
  }

  cargarCatalogos() {
    this.http.get<any>(`${this.apiUrl}/Camiones?limit=1000`).subscribe(res => {
        const data = res.data ?? res;
        this.camiones = data.$values ? data.$values : data;
    });
    this.http.get<any>(`${this.apiUrl}/Conductores?limit=1000`).subscribe(res => {
        const data = res.data ?? res;
        this.conductores = data.$values ? data.$values : data;
    });
    this.http.get<any>(`${this.apiUrl}/Almacenes?limit=1000`).subscribe(res => {
        const data = res.data ?? res;
        this.almacenes = data.$values ? data.$values : data;
    });
    this.http.get<any>(`${this.apiUrl}/Clientes`).subscribe(res => {
        const data = res.data ?? res;
        this.clientes = data.$values ? data.$values : data;
    });
    this.http.get<any>(`${this.apiUrl}/Pedidos?limit=2000`).subscribe(res => {
        const data = res.data ?? res;
        const allPedidos = data.$values ? data.$values : data;
        
        // Excluimos los pedidos que no tienen estado "Pendiente"
        this.pedidos = allPedidos.filter((p: any) => p.estado === 'Pendiente').map((p: any) => {
           let peso = 0;
           let volumen = 0;
           if (p.pedidoDetalles && p.pedidoDetalles.$values) {
              p.pedidoDetalles.$values.forEach((pd: any) => {
                 peso += (pd.producto?.pesoUnitario || 0) * pd.cantidad;
                 volumen += (pd.producto?.volumenUnitario || 0) * pd.cantidad;
              });
           } else if (p.pedidoDetalles && Array.isArray(p.pedidoDetalles)) {
              p.pedidoDetalles.forEach((pd: any) => {
                 peso += (pd.producto?.pesoUnitario || 0) * pd.cantidad;
                 volumen += (pd.producto?.volumenUnitario || 0) * pd.cantidad;
              });
           }
           p._pesoCalculado = peso;
           p._volumenCalculado = volumen;
           p._seleccionado = false;
           return p;
        });
    });
  }

  seleccionarCamion() {
    const camionEncontrado = this.camiones.find(c => c.id === this.formulario.camionId);
    this.camionSeleccionado = camionEncontrado || null;
    this.recalcular();
  }

  togglePedido(pedido: any, isChecked?: any) {
    if (isChecked !== undefined) {
      if (typeof isChecked === 'boolean') {
        pedido._seleccionado = isChecked;
      } else {
        pedido._seleccionado = isChecked.target?.checked === true;
      }
    } else {
      pedido._seleccionado = !pedido._seleccionado;
    }
    this.recalcular();
  }

  recalcular() {
    this.pesoLlenado = 0;
    this.volumenLlenado = 0;
    this.formulario.cargaPedidos = [];

    for (const p of this.pedidos) {
      if (p._seleccionado) {
        this.pesoLlenado += p._pesoCalculado;
        this.volumenLlenado += p._volumenCalculado;
        this.formulario.cargaPedidos.push({ pedidoId: p.id });
      }
    }
  }

  superaLimites(): boolean {
    if (!this.camionSeleccionado) return false;
    return this.pesoLlenado > this.camionSeleccionado.capacidadPeso ||
           this.volumenLlenado > this.camionSeleccionado.capacidadVolumen;
  }

  guardarCarga() {
     const payload = { ...this.formulario };
     if (payload.tipoDestino === 'cliente') {
        payload.destinoAlmacenId = '';
     } else {
        payload.destinoClienteId = '';
     }

     this.http.post<any>(`${this.apiUrl}/Cargas`, payload)
       .subscribe({
          next: () => {
             this.cerrarGenerador();
             this.cargarCargas(1, 50);
          },
          error: (err) => {
             const msg = err.error?.mensaje || 'Error al guardar la carga';
             alert('⚠️ ' + msg);
          }
       });
  }

  eliminarCarga(id: string) {
    if (confirm('¿Estás seguro de eliminar esta carga?')) {
      this.http.delete(`${this.apiUrl}/Cargas/${id}`)
        .subscribe({
          next: () => {
            this.cargas = this.cargas.filter(c => c.id !== id);
          },
          error: () => alert('Error al eliminar')
        });
    }
  }
}
