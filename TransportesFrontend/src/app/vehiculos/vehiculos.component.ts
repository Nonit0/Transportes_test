import { Component, OnInit } from '@angular/core';
import { VehiculoService } from '../services/vehiculo.service';
import { Camion, CreateCamion, UpdateCamion } from './camion/camion.model';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-vehiculos',
  templateUrl: './vehiculos.component.html',
  styleUrls: ['./vehiculos.component.css']
})
export class VehiculosComponent implements OnInit {
  camiones: Camion[] = [];
  nuevoCamion: CreateCamion = {
    matricula: '',
    capacidadPeso: 0,
    capacidadVolumen: 0,
    activo: true,
    clienteId: ''
  };
  
  camionEditando: Camion | null = null;
  mensajeError: string = '';

  get camionParaForm() {
    return this.camionEditando || this.nuevoCamion;
  }

  resetearFormulario() {
    this.camionEditando = null;
    this.nuevoCamion = {
      matricula: '',
      capacidadPeso: 0,
      capacidadVolumen: 0,
      activo: true,
      clienteId: ''
    };
    this.mensajeError = '';
  }

  isAdmin: boolean = false;
  clientes: any[] = [];
  private apiUrl = environment.apiUrl;
  
  totalItems: number = 0;
  cargando: boolean = true;

  constructor(private vehiculoService: VehiculoService, private http: HttpClient, private authService: AuthService) {
    this.isAdmin = this.authService.getRol() === 'Administrador' || this.authService.getRol() === 'Admin';
  }

  ngOnInit(): void {
    this.cargarCamiones();
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
    this.cargarCamiones(page, limit);
  }

  cargarCamiones(page: number = 1, limit: number = 50): void {
    this.vehiculoService.getCamiones(page, limit).subscribe({
      next: (res) => {
        const rawCamiones = res.items;
        this.totalItems = res.totalItems;
        // Normalización para que funcione con PascalCase o camelCase
        this.camiones = rawCamiones.map((c: any) => ({
          ...c,
          id: c.id || c.Id,
          matricula: c.matricula || c.Matricula,
          capacidadPeso: c.capacidadPeso || c.CapacidadPeso,
          capacidadVolumen: c.capacidadVolumen || c.CapacidadVolumen,
          activo: c.activo !== undefined ? c.activo : c.Activo,
          clienteId: c.clienteId || c.ClienteId || ''
        }));
        this.cargando = false;
      },
      error: (err) => {
        console.error('Error cargando camiones', err);
        this.mensajeError = 'No se pudieron cargar los camiones. Asegúrate de que el backend esté encendido.';
        this.cargando = false;
      }
    });
  }

  crearCamion(): void {
    this.mensajeError = '';
    if (!this.nuevoCamion.matricula || this.nuevoCamion.capacidadPeso <= 0 || this.nuevoCamion.capacidadVolumen <= 0) {
       this.mensajeError = 'Por favor, rellene todos los campos correctamente con valores mayores a 0.';
       return;
    }

    this.vehiculoService.createCamion(this.nuevoCamion).subscribe({
      next: (creado) => {
         this.camiones.unshift(creado); 
         this.nuevoCamion = { matricula: '', capacidadPeso: 0, capacidadVolumen: 0, activo: true, clienteId: '' };
      },
      error: (err) => {
         if (err.error && err.error.mensaje) {
            this.mensajeError = err.error.mensaje;
         } else {
            this.mensajeError = 'Error al crear el camión. Revisa la consola para más detalles.';
         }
      }
    });
  }

  iniciarEdicion(camion: Camion): void {
    this.camionEditando = { ...camion };
    this.mensajeError = '';
  }

  cancelarEdicion(): void {
    this.camionEditando = null;
    this.mensajeError = '';
  }

  guardarEdicion(): void {
    if (!this.camionEditando) return;

    this.vehiculoService.updateCamion(this.camionEditando.id!, this.camionEditando).subscribe({
      next: (actualizado) => {
         if (!actualizado) {
            this.cargarCamiones();
         } else {
            const res = actualizado as any;
            const normalizado = { ...res, id: res.id || res.Id };
            this.camiones = this.camiones.map(c => c.id === normalizado.id ? normalizado : c);
         }
         this.camionEditando = null;
         this.mensajeError = '';
      },
      error: (err) => {
         console.error('Detalle del error 400:', err.error); // Para ver qué campo falla
         if (err.error && err.error.mensaje) {
            this.mensajeError = err.error.mensaje;
         } else {
            this.mensajeError = 'Error al actualizar el camión. Revisa los detalles en la consola.';
         }
      }
    });
  }

  eliminarCamion(id: string): void {
    if (confirm('¿Estás seguro de que deseas desactivar este camión?')) {
      this.vehiculoService.deleteCamion(id).subscribe({
        next: () => {
          this.cargarCamiones();
        },
        error: (err) => {
          console.error(err);
          this.mensajeError = 'Error al desactivar el camión.';
        }
      });
    }
  }

  reactivarCamion(camion: Camion): void {
      const camionCompleto = { ...camion, activo: true };
      
      this.vehiculoService.updateCamion(camion.id!, camionCompleto).subscribe({
        next: (actualizado) => {
          if (!actualizado) {
            this.cargarCamiones();
          } else {
            const res = actualizado as any;
            const normalizado = { ...res, id: res.id || res.Id };
            this.camiones = this.camiones.map(c => c.id === normalizado.id ? normalizado : c);
          }
        },
        error: (err) => {
          console.error(err);
          this.mensajeError = 'Error al reactivar el camión.';
        }
      });
  }

  // ==========================================
  // 🚚 MÁSCARA DE MATRÍCULA (0000-XXX)
  // ==========================================
  onMatriculaChange(valor: string, tipo: 'crear' | 'editar') {
    if (!valor) return;

    // 1. Lo pasamos todo a mayúsculas para facilitar la vida
    let crudo = valor.toUpperCase();

    // 2. Extraemos a la fuerza solo los números (máximo 4)
    let numeros = crudo.replace(/[^0-9]/g, '').substring(0, 4);

    // 3. Extraemos a la fuerza solo las letras
    let partesLetras = crudo.replace(/[^A-Z]/g, '');
    let letras = '';
    
    // Solo le dejamos procesar letras si ya ha completado los 4 números
    if (numeros.length === 4) {
      letras = partesLetras.substring(0, 3); // Máximo 3 letras
    }

    // 4. Construimos el puzzle final
    let resultado = numeros;
    if (numeros.length === 4) {
      if (letras.length > 0) {
        resultado += '-' + letras;
      } else if (valor.endsWith('-')) {
        resultado += '-'; // Permite que el usuario escriba el guion a mano si quiere
      }
    }

    // 5. Devolvemos el valor limpio al formulario correspondiente
    if (tipo === 'crear') {
      // Reemplaza 'this.formulario' por el nombre de tu variable de creación
      this.nuevoCamion.matricula = resultado; 
    } else {
      this.camionEditando!.matricula = resultado;
    }
  }
}
