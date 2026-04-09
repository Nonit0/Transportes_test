import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-registrar',
  templateUrl: './registrar.component.html'
})
export class RegistrarComponent {
  // El rol 1 es 'Operario' según tu Enum en C#
  nuevoUsuario = { nombre: '', email: '', password: '', rol: 1, clienteId: null };
  errorMensaje = '';
  cargando = false;

  constructor(private authService: AuthService, private router: Router) {}

  hacerRegistro() {
    this.cargando = true;
    this.errorMensaje = '';

    this.authService.registrar(this.nuevoUsuario).subscribe({
      next: () => {
        this.cargando = false;
        // ¡Registro perfecto! Volvemos al login
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.cargando = false;
        this.errorMensaje = err.error.mensaje || 'Error al registrar el usuario.';
      }
    });
  }
}