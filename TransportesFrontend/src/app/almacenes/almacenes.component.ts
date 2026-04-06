import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Almacen, DireccionCombo } from './almacen.dto';

@Component({
  selector: 'app-almacenes',
  templateUrl: './almacenes.component.html',
  styleUrls: ['./almacenes.component.css']
})
export class AlmacenesComponent implements OnInit {
  almacenes: Almacen[] = [];
  direcciones: DireccionCombo[] = [];
  idEdicion: string | null = null;
  
  formulario = {
    nombre: '', direccionId: ''
  };

  // ==========================================
  // 🛸 VARIABLES DEL MODAL DE DIRECCIONES
  // ==========================================
  mostrarModal = false;
  nuevaDireccion = { calle: '', ciudad: '', cp: '', provincia: '', pais: '' };

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargarAlmacenes();
    this.cargarDirecciones();
  }

  // GET: Carga todos los almacenes
  cargarAlmacenes() {
    this.http.get<any>('https://localhost:5011/api/Almacenes')
      .subscribe({
        next: (data) => this.almacenes = data.$values ? data.$values : data,
        error: (err) => console.error('Error al cargar almacenes', err)
      });
  }

  // GET: Carga todas las direcciones para el Desplegable
  cargarDirecciones() {
    this.http.get<any>('https://localhost:5011/api/Direcciones')
      .subscribe({
        next: (data) => this.direcciones = data.$values ? data.$values : data,
        error: (err) => console.error('Error al cargar direcciones', err)
      });
  }

  // POST / PUT: Crea o Actualiza un almacén
  guardarAlmacen() {
    if (this.idEdicion) {
      this.http.put<any>(`https://localhost:5011/api/Almacenes/${this.idEdicion}`, this.formulario)
        .subscribe({
          next: (actualizado) => {
            // El backend puede devolver con $id wrapper del ReferenceHandler
            const almacen = actualizado.$values ? actualizado.$values : actualizado;
            this.almacenes = this.almacenes.map(e => e.id === almacen.id ? almacen : e);
            this.resetearFormulario();
          },
          error: (err) => console.error('Error al actualizar', err)
        });
    } else {
      this.http.post<any>('https://localhost:5011/api/Almacenes', this.formulario)
        .subscribe({
          next: (creado) => {
            this.almacenes.unshift(creado); 
            this.resetearFormulario();
          },
          error: (err) => console.error('Error al crear', err)
        });
    }
  }

  // DELETE: Elimina un almacén
  eliminarAlmacen(id: string, nombre: string) {
    const confirmacion = confirm(`¿Estás seguro de que quieres eliminar el almacén "${nombre}"?`);
    if (confirmacion) {
      this.http.delete(`https://localhost:5011/api/Almacenes/${id}`)
        .subscribe({
          next: () => {
            this.almacenes = this.almacenes.filter(a => a.id !== id);
          },
          error: (err) => alert('Hubo un problema al eliminar el almacén.')
        });
    }
  }

  // PUT: Prepara el formulario para editar
  cargarParaEdicion(elemento: Almacen) {
    this.idEdicion = elemento.id;
    this.formulario = { 
      nombre: elemento.nombre, 
      direccionId: elemento.direccionId
    };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetearFormulario() {
    this.idEdicion = null;
    this.formulario = { nombre: '', direccionId: '' };
  }

  // ==========================================
  // 🛸 MÉTODOS DEL MODAL DE DIRECCIONES
  // ==========================================
  abrirModal() {
    this.mostrarModal = true;
  }

  cerrarModal() {
    this.mostrarModal = false;
    this.nuevaDireccion = { calle: '', ciudad: '', cp: '', provincia: '', pais: '' };
  }

  guardarDireccion() {
    this.http.post<any>('https://localhost:5011/api/Direcciones', this.nuevaDireccion)
      .subscribe({
        next: (dirCreada) => {
          this.direcciones.push(dirCreada);
          this.formulario.direccionId = dirCreada.id;
          this.cerrarModal();
        },
        error: (err) => {
          console.error('Error al crear dirección', err);
          alert('Hubo un problema al crear la nueva dirección.');
        }
      });
  }
}