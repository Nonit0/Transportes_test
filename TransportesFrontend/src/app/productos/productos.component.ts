import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface ProductoDTO {
  id: string;
  nombre: string;
  descripcion: string;
  pesoUnitario: number;
  volumenUnitario: number;
}

export interface CreateProductoDTO {
  nombre: string;
  descripcion: string;
  pesoUnitario: number;
  volumenUnitario: number;
}

@Component({
  selector: 'app-productos',
  templateUrl: './productos.component.html',
  styleUrls: ['./productos.component.css']
})
export class ProductosComponent implements OnInit {
  productos: ProductoDTO[] = [];
  
  // Formulario de creación
  formulario: CreateProductoDTO = { nombre: '', descripcion: '', pesoUnitario: 0, volumenUnitario: 0 };

  // Variables para Edición Inline
  idEdicion: string | null = null;
  productoEnEdicion: any = {};

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargarProductos();
  }

  cargarProductos() {
    this.http.get<ProductoDTO[]>('https://localhost:5011/api/Productos')
      .subscribe({ next: (data) => this.productos = data });
  }

  guardarProducto() {
    this.http.post<ProductoDTO>('https://localhost:5011/api/Productos', this.formulario)
      .subscribe({
        next: (creado) => {
          this.productos.unshift(creado);
          this.formulario = { nombre: '', descripcion: '', pesoUnitario: 0, volumenUnitario: 0 }; // Reset
        },
        error: (err) => alert('Error al crear. ¿Quizás el nombre ya existe?')
      });
  }

  eliminarProducto(id: string, nombre: string) {
    if (confirm(`¿Eliminar definitivamente el producto "${nombre}"?`)) {
      this.http.delete(`https://localhost:5011/api/Productos/${id}`)
        .subscribe({ 
          next: () => this.productos = this.productos.filter(p => p.id !== id),
          error: (err) => {
            // Si el backend nos manda un mensaje explicativo, lo mostramos. Si no, mensaje genérico.
            const mensajeError = err.error?.mensaje || 'Hubo un error al eliminar el producto.';
            alert('⚠️ ' + mensajeError);
          }
        });
    }
  }

  // --- MÉTODOS DE EDICIÓN INLINE ---
  activarEdicion(producto: ProductoDTO) {
    this.idEdicion = producto.id;
    this.productoEnEdicion = { ...producto }; // Copia rápida del objeto
  }

  cancelarEdicion() {
    this.idEdicion = null;
    this.productoEnEdicion = {};
  }

  guardarEdicion(id: string) {
    this.http.put(`https://localhost:5011/api/Productos/${id}`, this.productoEnEdicion)
      .subscribe({
        next: () => {
          // Actualizamos la tabla visualmente
          this.productos = this.productos.map(p => p.id === id ? { ...this.productoEnEdicion, id } : p);
          this.cancelarEdicion();
        },
        error: (err) => alert('Error al actualizar el producto.')
      });
  }
}
