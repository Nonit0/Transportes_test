import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Almacen, DireccionCombo } from './almacen.model';
import { environment } from '../../environments/environment';

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

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargarAlmacenes();
    this.cargarDirecciones();
  }

  // GET: Carga todos los almacenes
  cargarAlmacenes() {
    this.http.get<any>(`${this.apiUrl}/Almacenes`)
      .subscribe({
        next: (data) => {
          this.almacenes = data.$values ? data.$values : data;
        },
        error: (err) => console.error('Error al cargar almacenes', err)
      });
  }

  // GET:c Carga todas las direcciones para el Desplegable
  cargarDirecciones() {
    this.http.get<any>(`${this.apiUrl}/Direcciones`)
      .subscribe({
        next: (data) => {
          const raw = data.$values ? data.$values : data;
          // Generamos textoMostrar si el backend no lo envía (o usamos el del backend si lo hace)
          this.direcciones = raw.map((d: any) => ({
            ...d,
            textoMostrar: d.textoMostrar || `${d.calle}, ${d.ciudad} (${d.cp})`
          }));
        },
        error: (err) => console.error('Error al cargar direcciones', err)
      });
  }

  // POST / PUT: Crea o Actualiza un almacén
  guardarAlmacen() {
    if (this.idEdicion) {
      this.http.put<any>(`${this.apiUrl}/Almacenes/${this.idEdicion}`, this.formulario)
        .subscribe({
          next: (actualizado) => {
            // Actualizamos localmente el almacén modificado
            this.almacenes = this.almacenes.map(a => a.id === actualizado.id ? actualizado : a);
            this.resetearFormulario();
          },
          error: (err) => console.error('Error al actualizar', err)
        });
    } else {
      this.http.post<any>(`${this.apiUrl}/Almacenes`, this.formulario)
        .subscribe({
          next: (creado) => {
            // Añadimos el nuevo al principio para verlo de inmediato
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
      this.http.delete(`${this.apiUrl}/Almacenes/${id}`)
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
    this.http.post<any>(`${this.apiUrl}/Direcciones`, this.nuevaDireccion)
      .subscribe({
        next: (dirCreada) => {
          // Generamos el texto para que el selector lo muestre correctamente de inmediato
          const nuevaDir = {
            ...dirCreada,
            textoMostrar: `${dirCreada.calle}, ${dirCreada.ciudad} (${dirCreada.cp})`
          };
          this.direcciones.push(nuevaDir);
          this.formulario.direccionId = nuevaDir.id;
          this.cerrarModal();
        },
        error: (err) => {
          console.error('Error al crear dirección', err);
          alert('Hubo un problema al crear la nueva dirección.');
        }
      });
  }
}