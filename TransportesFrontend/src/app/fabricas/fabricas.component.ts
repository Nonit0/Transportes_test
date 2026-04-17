import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';

export interface Fabrica {
  id: string;
  nombre: string;
  direccionId: string;
  deletedAt: string | null;
  direccion: any;
  clienteId?: string;
}

export interface DireccionCombo {
  id: string;
  textoMostrar: string;
}

@Component({
  selector: 'app-fabricas',
  templateUrl: './fabricas.component.html',
  styleUrls: ['./fabricas.component.css']
})
export class FabricasComponent implements OnInit {
  fabricas: Fabrica[] = [];
  direcciones: DireccionCombo[] = [];

  formulario = { nombre: '', direccionId: '', clienteId: '' };

  idEdicion: string | null = null;
  
  mostrarModal = false;
  nuevaDireccion = { calle: '', ciudad: '', cp: '', provincia: '', pais: '' };

  private apiUrl = environment.apiUrl;
  isAdmin: boolean = false;
  clientes: any[] = [];
  
  cargando = true;
  totalItems = 0;

  constructor(private http: HttpClient, private authService: AuthService) {
    this.isAdmin = this.authService.getRol() === 'Administrador' || this.authService.getRol() === 'Admin';
  }

  ngOnInit(): void {
    this.cargarFabricas();
    this.cargarDirecciones();
    if (this.isAdmin) this.cargarClientes();
  }

  cargarClientes() {
    this.http.get<any>(`${this.apiUrl}/Clientes`)
      .subscribe({
        next: (res) => {
          const responseData = res.data ?? res;
          this.clientes = responseData.$values ? responseData.$values : responseData;
        },
        error: (err) => console.error('Error al cargar clientes', err)
      });
  }

  refresh(state: any) {
    this.cargando = true;
    const page = state.page ? state.page.current : 1;
    const limit = state.page ? state.page.size : 50;
    this.cargarFabricas(page, limit);
  }

  cargarFabricas(page: number = 1, limit: number = 50) {
    this.http.get<any>(`${this.apiUrl}/Fabricas?page=${page}&limit=${limit}`)
      .subscribe({
        next: (res) => {
           const responseData = res.data ?? res;
           this.fabricas = responseData.$values ? responseData.$values : responseData;
           this.totalItems = res.totalItems !== undefined ? res.totalItems : this.fabricas.length;
           this.cargando = false;
        },
        error: (err) => {
           console.error('Error al cargar fábricas', err);
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

  guardarFabrica() {
    if (this.idEdicion) {
      this.http.put<any>(`${this.apiUrl}/Fabricas/${this.idEdicion}`, this.formulario)
        .subscribe({
          next: (actualizada) => {
            this.fabricas = this.fabricas.map(f => f.id === actualizada.id ? actualizada : f);
            this.resetearFormulario();
          },
          error: (err) => {
            const msg = err.error?.mensaje || 'Error al actualizar la fábrica.';
            alert('⚠️ ' + msg);
          }
        });
    } else {
      this.http.post<any>(`${this.apiUrl}/Fabricas`, this.formulario)
        .subscribe({
          next: (creada) => {
            this.fabricas.unshift(creada);
            this.resetearFormulario();
          },
          error: (err) => {
            const msg = err.error?.mensaje || 'Error al crear la fábrica.';
            alert('⚠️ ' + msg);
          }
        });
    }
  }

  eliminarFabrica(id: string, nombre: string) {
    if (confirm(`¿Eliminar definitivamente la fábrica "${nombre}"?`)) {
      this.http.delete(`${this.apiUrl}/Fabricas/${id}`)
        .subscribe({
          next: () => this.fabricas = this.fabricas.filter(c => c.id !== id),
          error: () => alert('Hubo un problema al eliminar la fábrica.')
        });
    }
  }

  cargarParaEdicion(fabrica: Fabrica) {
    this.idEdicion = fabrica.id;
    this.formulario = {
      nombre: fabrica.nombre,
      direccionId: fabrica.direccionId,
      clienteId: fabrica.clienteId || ''
    };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetearFormulario() {
    this.idEdicion = null;
    this.formulario = { nombre: '', direccionId: '', clienteId: '' };
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
