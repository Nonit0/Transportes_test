import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface Fabrica {
  id: string;
  nombre: string;
  direccionId: string;
  deletedAt: string | null;
  direccion: any;
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

  formulario = { nombre: '', direccionId: '' };

  idEdicion: string | null = null;
  
  mostrarModal = false;
  nuevaDireccion = { calle: '', ciudad: '', cp: '', provincia: '', pais: '' };

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargarFabricas();
    this.cargarDirecciones();
  }

  cargarFabricas() {
    this.http.get<any>(`${this.apiUrl}/Fabricas`)
      .subscribe({
        next: (data) => this.fabricas = data.$values ? data.$values : data,
        error: (err) => console.error('Error al cargar fábricas', err)
      });
  }

  cargarDirecciones() {
    this.http.get<any>(`${this.apiUrl}/Direcciones`)
      .subscribe({
        next: (data) => this.direcciones = data.$values ? data.$values : data,
        error: (err) => console.error('Error al cargar direcciones', err)
      });
  }

  guardarFabrica() {
    if (this.idEdicion) {
      this.http.put<any>(`${this.apiUrl}/Fabricas/${this.idEdicion}`, this.formulario)
        .subscribe({
          next: () => {
            this.cargarFabricas();
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
          next: () => {
            this.cargarFabricas();
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
      direccionId: fabrica.direccionId
    };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetearFormulario() {
    this.idEdicion = null;
    this.formulario = { nombre: '', direccionId: '' };
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
          this.direcciones.push(dirCreada);
          this.formulario.direccionId = dirCreada.id;
          this.cerrarModal();
        },
        error: () => alert('Hubo un problema al crear la nueva dirección.')
      });
  }
}
