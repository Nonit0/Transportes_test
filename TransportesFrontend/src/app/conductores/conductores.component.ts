import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface Conductor {
  id: string;
  dni: string;
  nombre: string;
  apellidos: string;
  telefono: string;
  deletedAt: string | null;
}

@Component({
  selector: 'app-conductores',
  templateUrl: './conductores.component.html',
  styleUrls: ['./conductores.component.css']
})
export class ConductoresComponent implements OnInit {
  conductores: Conductor[] = [];

  // Formulario de creación
  formulario = { dni: '', nombre: '', apellidos: '', telefono: '' };

  // Variables para Edición Inline
  idEdicion: string | null = null;
  conductorEnEdicion: any = {};

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargarConductores();
  }

  cargarConductores() {
    this.http.get<any>(`${this.apiUrl}/Conductores`)
      .subscribe({ next: (data) => this.conductores = data.$values ? data.$values : data });
  }

  guardarConductor() {
    this.http.post<Conductor>(`${this.apiUrl}/Conductores`, this.formulario)
      .subscribe({
        next: (creado) => {
          this.conductores.unshift(creado);
          this.formulario = { dni: '', nombre: '', apellidos: '', telefono: '' };
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
