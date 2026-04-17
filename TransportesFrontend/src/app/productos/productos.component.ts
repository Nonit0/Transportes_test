import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';

export interface Producto {
  id: string;
  nombre: string;
  descripcion: string;
  pesoUnitario: number;
  volumenUnitario: number;
  clienteId?: string;
}

export interface CreateProducto {
  nombre: string;
  descripcion: string;
  pesoUnitario: number;
  volumenUnitario: number;
  clienteId?: string;
}

@Component({
  selector: 'app-productos',
  templateUrl: './productos.component.html',
  styleUrls: ['./productos.component.css']
})
export class ProductosComponent implements OnInit {
  productos: Producto[] = [];
  
  // Formulario de creación
  formulario: CreateProducto = { nombre: '', descripcion: '', pesoUnitario: 0, volumenUnitario: 0, clienteId: '' };

  // Variables para Edición Inline
  idEdicion: string | null = null;
  productoEnEdicion: any = {};

  private apiUrl = environment.apiUrl;
  isAdmin: boolean = false;
  clientes: any[] = [];
  
  cargando = true;
  totalItems = 0;

  constructor(private http: HttpClient, private authService: AuthService) {
    this.isAdmin = this.authService.getRol() === 'Administrador' || this.authService.getRol() === 'Admin';
  }

  ngOnInit(): void {
    this.cargarProductos();
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
    this.cargarProductos(page, limit);
  }

  cargarProductos(page: number = 1, limit: number = 50) {
    this.http.get<any>(`${this.apiUrl}/Productos?page=${page}&limit=${limit}`)
      .subscribe({ 
        next: (res) => {
          const responseData = res.data ?? res;
          this.productos = responseData.$values ? responseData.$values : responseData;
          this.totalItems = res.totalItems !== undefined ? res.totalItems : this.productos.length;
          this.cargando = false;
        },
        error: (err) => {
          console.error(err);
          this.cargando = false;
        }
      });
  }

  guardarProducto() {
    this.http.post<Producto>(`${this.apiUrl}/Productos`, this.formulario)
      .subscribe({
        next: (creado) => {
          this.productos.unshift(creado);
          this.formulario = { nombre: '', descripcion: '', pesoUnitario: 0, volumenUnitario: 0, clienteId: '' }; // Reset
        },
        error: (err) => alert('Error al crear. ¿Quizás el nombre ya existe?')
      });
  }

  eliminarProducto(id: string, nombre: string) {
    if (confirm(`¿Eliminar definitivamente el producto "${nombre}"?`)) {
      this.http.delete(`${this.apiUrl}/Productos/${id}`)
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
  activarEdicion(producto: Producto) {
    this.idEdicion = producto.id;
    this.productoEnEdicion = { ...producto }; // Copia rápida del objeto
  }

  cancelarEdicion() {
    this.idEdicion = null;
    this.productoEnEdicion = {};
  }

  guardarEdicion(id: string) {
    this.http.put(`${this.apiUrl}/Productos/${id}`, this.productoEnEdicion)
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
