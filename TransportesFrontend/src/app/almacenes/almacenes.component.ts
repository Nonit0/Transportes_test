import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AlmacenDTO, CreateAlmacenDTO } from '../almacen.dto'; // <-- OJO: Añadimos '../' porque hemos bajado un nivel

@Component({
  selector: 'app-almacenes',
  templateUrl: './almacenes.component.html',
  styleUrls: ['./almacenes.component.css']
})
export class AlmacenesComponent implements OnInit {
  almacenes: AlmacenDTO[] = [];
  idEdicion: string | null = null;
  
  formulario: CreateAlmacenDTO = {
    nombre: '', direccionCompleta: '', ciudad: '', cp: '', provincia: '', pais: ''
  };

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargarAlmacenes();
  }

  cargarAlmacenes() {
    this.http.get<AlmacenDTO[]>('https://localhost:5011/api/Almacenes')
      .subscribe({
        next: (data) => this.almacenes = data,
        error: (err) => console.error('Error al cargar', err)
      });
  }

  guardarAlmacen() {
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

  cargarParaEdicion(elemento: AlmacenDTO) {
    this.idEdicion = elemento.id;
    this.formulario = { 
      nombre: elemento.nombre, 
      direccionCompleta: elemento.direccionCompleta,
      ciudad: elemento.ciudad,
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