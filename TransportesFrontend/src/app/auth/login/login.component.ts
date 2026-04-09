import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent {
  // Objeto idéntico a la clase 'Credenciales' de tu AuthController en C#
  credenciales = { email: '', password: '' };
  errorMensaje = '';
  cargando = false;

  constructor(private authService: AuthService, private router: Router) {}

  hacerLogin() {
    this.cargando = true;
    this.errorMensaje = '';

    this.authService.login(this.credenciales).subscribe({
      next: (respuesta) => {
        this.cargando = false;
        // Si el login es un éxito, Angular nos lleva a la página de pedidos
        this.router.navigate(['/pedidos']); 
      },
      error: (err) => {
        this.cargando = false;
        // Capturamos el mensaje de error que configuramos en el backend
        this.errorMensaje = err.error.mensaje || 'Error al conectar con el servidor.';
      }
    });
  }
}