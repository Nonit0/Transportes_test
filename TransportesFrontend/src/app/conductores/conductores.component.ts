import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';

export interface Conductor {
  id: string;
  dni: string;
  nombre: string;
  apellidos: string;
  telefono: string;
  deletedAt: string | null;
  clienteId?: string;
}

@Component({
  selector: 'app-conductores',
  templateUrl: './conductores.component.html',
  styleUrls: ['./conductores.component.css']
})
export class ConductoresComponent implements OnInit {
  conductores: Conductor[] = [];

  // Formulario de creación
  formulario = { dni: '', nombre: '', apellidos: '', telefono: '', clienteId: '' };

  // Variables para Edición Inline
  idEdicion: string | null = null;
  conductorEnEdicion: any = {};

  private apiUrl = environment.apiUrl;
  isAdmin: boolean = false;
  clientes: any[] = [];
  
  cargando = true;
  totalItems = 0;

  constructor(private http: HttpClient, private authService: AuthService) {
    this.isAdmin = this.authService.getRol() === 'Administrador' || this.authService.getRol() === 'Admin';
  }

  ngOnInit(): void {
    this.cargarConductores();
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
    this.cargarConductores(page, limit);
  }

  cargarConductores(page: number = 1, limit: number = 50) {
    this.http.get<any>(`${this.apiUrl}/Conductores?page=${page}&limit=${limit}`)
      .subscribe({ 
        next: (res) => {
          const responseData = res.data ?? res;
          this.conductores = responseData.$values ? responseData.$values : responseData;
          this.totalItems = res.totalItems !== undefined ? res.totalItems : this.conductores.length;
          this.cargando = false;
        },
        error: (err) => {
          console.error(err);
          this.cargando = false;
        }
      });
  }

  guardarConductor() {
    this.http.post<Conductor>(`${this.apiUrl}/Conductores`, this.formulario)
      .subscribe({
        next: (creado) => {
          this.conductores.unshift(creado);
          this.formulario = { dni: '', nombre: '', apellidos: '', telefono: '', clienteId: '' };
        },
        error: (err) => {
          const msg = err.error?.mensaje || 'Error al crear el conductor.';
          alert('⚠️ ' + msg);
        }
      });
  }

  eliminarConductor(id: string, nombre: string) {
    if (confirm(`¿Eliminar definitivamente al conductor "${nombre}"?`)) {
      this.http.delete(`${this.apiUrl}/Conductores/${id}`)
        .subscribe({
          next: () => this.conductores = this.conductores.filter(c => c.id !== id),
          error: (err) => {
            const msg = err.error?.mensaje || 'Error al eliminar el conductor.';
            alert('⚠️ ' + msg);
          }
        });
    }
  }

  // --- EDICIÓN INLINE ---
  activarEdicion(conductor: Conductor) {
    this.idEdicion = conductor.id;
    this.conductorEnEdicion = { ...conductor };
  }

  cancelarEdicion() {
    this.idEdicion = null;
    this.conductorEnEdicion = {};
  }

  guardarEdicion(id: string) {
    this.http.put(`${this.apiUrl}/Conductores/${id}`, this.conductorEnEdicion)
      .subscribe({
        next: () => {
          this.conductores = this.conductores.map(c => c.id === id ? { ...this.conductorEnEdicion, id } : c);
          this.cancelarEdicion();
        },
        error: (err) => {
          const msg = err.error?.mensaje || 'Error al actualizar el conductor.';
          alert('⚠️ ' + msg);
        }
      });
  }
}
