import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): boolean {
    // Si el usuario está logueado, le dejamos pasar
    if (this.authService.estaLogueado()) {
      return true;
    }

    // Si NO está logueado, lo mandamos a la raíz (Inicio)
    this.router.navigate(['/']);
    return false;
  }
}
