import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Almacen, DireccionCombo } from './almacen.model';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';

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
    nombre: '', direccionId: '', clienteId: ''
  };

  // ==========================================
  // 🛸 VARIABLES DEL MODAL DE DIRECCIONES
  // ==========================================
  mostrarModal = false;
  nuevaDireccion = { calle: '', ciudad: '', cp: '', provincia: '', pais: '' };
  
  // Paginación
  totalItems: number = 0;
  cargando: boolean = true;

  private apiUrl = environment.apiUrl;
  isAdmin: boolean = false;
  clientes: any[] = [];

  constructor(private http: HttpClient, private authService: AuthService) {
    this.isAdmin = this.authService.getRol() === 'Administrador' || this.authService.getRol() === 'Admin';
  }

  ngOnInit(): void {
    this.cargarAlmacenes();
    this.cargarDirecciones();
    if (this.isAdmin) this.cargarClientes();
  }

  // Evento Refresh de Clarity Datagrid para Server-Side Pagination
  refresh(state: any) {
    this.cargando = true;
    const page = state.page ? state.page.current : 1;
    const limit = state.page ? state.page.size : 50;
    this.cargarAlmacenes(page, limit);
  }

  // GET: Carga todos los almacenes (paginado)
  cargarAlmacenes(page: number = 1, limit: number = 50) {
    this.http.get<any>(`${this.apiUrl}/Almacenes?page=${page}&limit=${limit}`)
      .subscribe({
        next: (res) => {
          const responseData = res.data ?? res;
          this.almacenes = responseData.$values ? responseData.$values : responseData;
          this.totalItems = res.totalItems !== undefined ? res.totalItems : this.almacenes.length;
          this.cargando = false;
        },
        error: (err) => {
          console.error('Error al cargar almacenes', err);
          this.cargando = false;
        }
      });
  }

  // GET:c Carga todas las direcciones para el Desplegable
  cargarDirecciones() {
    this.http.get<any>(`${this.apiUrl}/Direcciones`)
      .subscribe({
        next: (res) => {
          const responseData = res.data ?? res;
          const raw = responseData.$values ? responseData.$values : responseData;
          // Generamos textoMostrar si el backend no lo envía (o usamos el del backend si lo hace)
          this.direcciones = raw.map((d: any) => ({
            ...d,
            textoMostrar: d.textoMostrar || `${d.calle}, ${d.ciudad} (${d.cp})`
          }));
        },
        error: (err) => console.error('Error al cargar direcciones', err)
      });
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
      direccionId: elemento.direccionId,
      clienteId: elemento.clienteId || ''
    };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetearFormulario() {
    this.idEdicion = null;
    this.formulario = { nombre: '', direccionId: '', clienteId: '' };
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