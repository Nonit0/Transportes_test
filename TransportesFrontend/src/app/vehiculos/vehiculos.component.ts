import { Component, OnInit } from '@angular/core';
import { VehiculoService } from '../services/vehiculo.service';
import { Camion, CreateCamion, UpdateCamion } from './camion/camion.model';

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
    activo: true
  };
  
  camionEditando: Camion | null = null;
  mensajeError: string = '';

  constructor(private vehiculoService: VehiculoService) {}

  ngOnInit(): void {
    this.cargarCamiones();
  }

  cargarCamiones(): void {
    this.vehiculoService.getCamiones().subscribe({
      next: (data) => {
        this.camiones = data;
      },
      error: (err) => {
        console.error('Error cargando camiones', err);
        this.mensajeError = 'No se pudieron cargar los camiones. Asegúrate de que el backend esté encendido.';
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
      next: () => {
         this.cargarCamiones(); 
         this.nuevoCamion = { matricula: '', capacidadPeso: 0, capacidadVolumen: 0, activo: true };
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

    const payload: UpdateCamion = {
      matricula: this.camionEditando.matricula,
      capacidadPeso: this.camionEditando.capacidadPeso,
      capacidadVolumen: this.camionEditando.capacidadVolumen,
      activo: this.camionEditando.activo
    };

    this.vehiculoService.updateCamion(this.camionEditando.id!, payload).subscribe({
      next: () => {
         this.cargarCamiones();
         this.camionEditando = null;
         this.mensajeError = '';
      },
      error: (err) => {
         if (err.error && err.error.mensaje) {
            this.mensajeError = err.error.mensaje;
         } else {
            this.mensajeError = 'Error al actualizar el camión.';
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
      const payload: UpdateCamion = {
        matricula: camion.matricula,
        capacidadPeso: camion.capacidadPeso,
        capacidadVolumen: camion.capacidadVolumen,
        activo: true // Reactivamos
      };
      this.vehiculoService.updateCamion(camion.id!, payload).subscribe({
        next: () => this.cargarCamiones(),
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
