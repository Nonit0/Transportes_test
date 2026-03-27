import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AlmacenDTO, CreateAlmacenDTO, DireccionDTO } from './almacen.dto';

@Component({
  selector: 'app-almacenes',
  templateUrl: './almacenes.component.html',
  styleUrls: ['./almacenes.component.css']
})
export class AlmacenesComponent implements OnInit {
  almacenes: AlmacenDTO[] = [];
  direcciones: DireccionDTO[] = [];
  idEdicion: string | null = null;
  
  formulario: CreateAlmacenDTO = {
    nombre: '', direccionId: ''
    /* Ya no hacen falta pues estos pertenecen a direccion no a almacen
    , ciudad: '', cp: '', provincia: '', pais: ''
    */
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
    this.http.get<AlmacenDTO[]>('https://localhost:5011/api/Almacenes')
      .subscribe({
        next: (data) => this.almacenes = data,
        error: (err) => console.error('Error al cargar almacenes', err)
      });
  }

  // GET: Carga todas las direcciones para el Desplegable
  cargarDirecciones() {
    this.http.get<DireccionDTO[]>('https://localhost:5011/api/Direcciones')
      .subscribe({
        next: (data) => this.direcciones = data,
        error: (err) => console.error('Error al cargar direcciones', err)
      });
  }

  // POST / PUT: Crea o Actualiza un almacén
  guardarAlmacen() {
    // debugg console.log('📦 Datos a enviar al servidor:', this.formulario);
    if (this.idEdicion) {
      this.http.put<AlmacenDTO>(`https://localhost:5011/api/Almacenes/${this.idEdicion}`, this.formulario)
        .subscribe({
          next: (actualizado) => {
            this.almacenes = this.almacenes.map(e => e.id === actualizado.id ? actualizado : e);
            this.resetearFormulario();
          },
          error: (err) => console.error('Error al actualizar', err)
        });
    } else {
      this.http.post<AlmacenDTO>('https://localhost:5011/api/Almacenes', this.formulario)
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
  cargarParaEdicion(elemento: AlmacenDTO) {
    this.idEdicion = elemento.id;
    this.formulario = { 
      nombre: elemento.nombre, 
      direccionId: elemento.direccionId // Selecciona automáticamente el valor en el <select>
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
    // Llamamos al Backend para crear la dirección
    this.http.post<DireccionDTO>('https://localhost:5011/api/Direcciones', this.nuevaDireccion)
      .subscribe({
        next: (dirCreada) => {
          // 1. Añadimos la dirección al array para que aparezca en el desplegable
          this.direcciones.push(dirCreada);
          // 2. La dejamos seleccionada en el formulario del almacén por comodidad
          this.formulario.direccionId = dirCreada.id;
          // 3. Cerramos el modal
          this.cerrarModal();
        },
        error: (err) => {
          console.error('Error al crear dirección', err);
          alert('Hubo un problema al crear la nueva dirección.');
        }
      });
  }
}