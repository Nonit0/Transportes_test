import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    // 1. ¿Está logueado?
    if (!this.authService.estaLogueado()) {
      this.router.navigate(['/']);
      return false;
    }

    // 2. ¿Tiene el rol necesario?
    const rolesEsperados = route.data['expectedRoles'] as Array<string>;
    const rolUsuario = this.authService.getRol();

    // Si la ruta no especifica roles, dejamos pasar (solo requiere estar logueado)
    if (!rolesEsperados || rolesEsperados.length === 0) {
      return true;
    }

    // Si el rol del usuario está en la lista de permitidos, dejamos pasar
    if (rolUsuario && rolesEsperados.includes(rolUsuario)) {
      return true;
    }

    // Si no tiene permiso, lo mandamos al inicio
    alert('No tienes permiso para acceder a esta sección.');
    this.router.navigate(['/']);
    return false;
  }
}
