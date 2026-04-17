import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface Cliente {
  id: string;
  nombre: string;
  direccionId: string;
  telefono: string;
  email: string;
  prefijo: string;
  deletedAt: string | null;
  direccion: any;
}

export interface DireccionCombo {
  id: string;
  textoMostrar: string;
}

@Component({
  selector: 'app-clientes',
  templateUrl: './clientes.component.html',
  styleUrls: ['./clientes.component.css']
})
export class ClientesComponent implements OnInit {
  clientes: Cliente[] = [];
  direcciones: DireccionCombo[] = [];

  formulario = { nombre: '', direccionId: '', telefono: '', email: '', prefijo: '+34' };

  prefijos = [
    { codigo: '+34', pais: '🇪🇸 +34' },
    { codigo: '+44', pais: '🇬🇧 +44' },
    { codigo: '+33', pais: '🇫🇷 +33' },
    { codigo: '+49', pais: '🇩🇪 +49' },
    { codigo: '+39', pais: '🇮🇹 +39' },
    { codigo: '+351', pais: '🇵🇹 +351' },
    { codigo: '+1', pais: '🇺🇸 +1' },
    { codigo: '+52', pais: '🇲🇽 +52' },
    { codigo: '+54', pais: '🇦🇷 +54' },
    { codigo: '+57', pais: '🇨🇴 +57' }
  ];

  // Edición inline
  idEdicion: string | null = null;
  clienteEnEdicion: any = {};

  // Modal de direcciones
  mostrarModal = false;
  nuevaDireccion = { calle: '', ciudad: '', cp: '', provincia: '', pais: '' };

  private apiUrl = environment.apiUrl;

  cargando = true;
  totalItems = 0;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargarClientes();
    this.cargarDirecciones();
  }

  refresh(state: any) {
    this.cargando = true;
    const page = state.page ? state.page.current : 1;
    const limit = state.page ? state.page.size : 50;
    this.cargarClientes(page, limit);
  }

  cargarClientes(page: number = 1, limit: number = 50) {
    this.http.get<any>(`${this.apiUrl}/Clientes?page=${page}&limit=${limit}`)
      .subscribe({
        next: (res) => {
           const responseData = res.data ?? res;
           this.clientes = responseData.$values ? responseData.$values : responseData;
           this.totalItems = res.totalItems !== undefined ? res.totalItems : this.clientes.length;
           this.cargando = false;
        },
        error: (err) => {
           console.error('Error al cargar clientes', err);
           this.cargando = false;
        }
      });
  }

  cargarDirecciones() {
    this.http.get<any>(`${this.apiUrl}/Direcciones`)
      .subscribe({
        next: (res) => {
          const responseData = res.data ?? res;
          const raw = responseData.$values ? responseData.$values : responseData;
          this.direcciones = raw.map((d: any) => ({
            ...d,
            textoMostrar: `${d.calle} - ${d.ciudad} (${d.cp})`
          }));
        },
        error: (err) => console.error('Error al cargar direcciones', err)
      });
  }

  guardarCliente() {
    if (this.idEdicion) {
      this.http.put<any>(`${this.apiUrl}/Clientes/${this.idEdicion}`, this.formulario)
        .subscribe({
          next: (actualizado) => {
            this.clientes = this.clientes.map(c => c.id === actualizado.id ? actualizado : c);
            this.resetearFormulario();
          },
          error: (err) => {
            const msg = err.error?.mensaje || 'Error al actualizar el cliente.';
            alert('⚠️ ' + msg);
          }
        });
    } else {
      this.http.post<any>(`${this.apiUrl}/Clientes`, this.formulario)
        .subscribe({
          next: (creado) => {
            this.clientes.unshift(creado);
            this.resetearFormulario();
          },
          error: (err) => {
            const msg = err.error?.mensaje || 'Error al crear el cliente.';
            alert('⚠️ ' + msg);
          }
        });
    }
  }

  eliminarCliente(id: string, nombre: string) {
    if (confirm(`¿Eliminar definitivamente al cliente "${nombre}"?`)) {
      this.http.delete(`${this.apiUrl}/Clientes/${id}`)
        .subscribe({
          next: () => this.clientes = this.clientes.filter(c => c.id !== id),
          error: () => alert('Hubo un problema al eliminar el cliente.')
        });
    }
  }

  cargarParaEdicion(cliente: Cliente) {
    this.idEdicion = cliente.id;
    this.formulario = {
      nombre: cliente.nombre,
      direccionId: cliente.direccionId,
      telefono: cliente.telefono,
      email: cliente.email,
      prefijo: cliente.prefijo || '+34'
    };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetearFormulario() {
    this.idEdicion = null;
    this.formulario = { nombre: '', direccionId: '', telefono: '', email: '', prefijo: '+34' };
  }

  // ==========================================
  // 🛸 MODAL DE DIRECCIONES
  // ==========================================
  abrirModal() {
    this.mostrarModal = true;
  }

  cerrarModal() {
    this.mostrarModal = false;
    this.nuevaDireccion = { calle: '', ciudad: '', cp: '', provincia: '', pais: '' };
  }

  guardarDireccion() {
    this.http.post<any>(`${this.apiUrl}/Direcciones`, this.nuevaDireccion)
      .subscribe({
        next: (dirCreada) => {
          const nueva = {
             ...dirCreada,
             textoMostrar: `${dirCreada.calle} - ${dirCreada.ciudad} (${dirCreada.cp})`
          };
          this.direcciones.push(nueva);
          this.formulario.direccionId = nueva.id;
          this.cerrarModal();
        },
        error: () => alert('Hubo un problema al crear la nueva dirección.')
      });
  }
}
