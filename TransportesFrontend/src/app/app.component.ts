import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AlmacenDTO, CreateAlmacenDTO } from './almacen.dto'; 

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  // Lista visual de almacenes
  almacenes: AlmacenDTO[] = [];

  // Variables para el modo edición
  idEdicion: string | null = null;
  
  // UNIFICADO: Este es el único objeto que controla el formulario
  formulario: CreateAlmacenDTO = {
    nombre: '', direccionCompleta: '', ciudad: '', cp: '', provincia: '', pais: ''
  };

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargarAlmacenes();
  }

  // 1. GET (Read)
  cargarAlmacenes() {
    this.http.get<AlmacenDTO[]>('https://localhost:5011/api/Almacenes')
      .subscribe({
        next: (data) => this.almacenes = data,
        error: (err) => console.error('Error al cargar', err)
      });
  }

  // 2. POST / PUT (Create / Update) - ¡UNIFICADO!
  guardarAlmacen() {
    if (this.idEdicion) {
      // 🔵 MODO EDICIÓN (PUT)
      this.http.put<AlmacenDTO>(`https://localhost:5011/api/Almacenes/${this.idEdicion}`, this.formulario)
        .subscribe({
          next: (actualizado) => {
            // Reemplazamos el viejo por el actualizado en la lista
            this.almacenes = this.almacenes.map(e => e.id === actualizado.id ? actualizado : e);
            this.resetearFormulario();
          },
          error: (err) => console.error('Error al actualizar', err)
        });
    } else {
      // 🟢 MODO CREACIÓN (POST)
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

  // 3. DELETE (Soft Delete)
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

  // 4. Utilidades de Interfaz (Preparar edición y limpiar)
  cargarParaEdicion(elemento: AlmacenDTO) {
    this.idEdicion = elemento.id;
    
    // Mapeamos los datos de la tabla al formulario
    this.formulario = { 
      nombre: elemento.nombre, 
      direccionCompleta: elemento.direccionCompleta,
      ciudad: elemento.ciudad,
      // Usamos 'any' por si AlmacenDTO aún no tiene definidos estos campos
      cp: (elemento as any).cp || '',
      provincia: (elemento as any).provincia || '',
      pais: (elemento as any).pais || ''
    };
    
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetearFormulario() {
    this.idEdicion = null;
    this.formulario = { nombre: '', direccionCompleta: '', ciudad: '', cp: '', provincia: '', pais: '' };
  }
}