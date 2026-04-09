import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl + '/auth';

  constructor(private http: HttpClient) { }

  // 1. LOGIN
  login(credenciales: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, credenciales).pipe(
      tap((respuesta: any) => {
        // Si C# nos devuelve un token, lo guardamos en el LocalStorage
        if (respuesta && respuesta.token) {
          localStorage.setItem('token', respuesta.token);
        }
      })
    );
  }

  // 2. REGISTRO
  registrar(usuario: any): Observable<any> {
    // Aquí no guardamos token porque el registro solo crea la cuenta, 
    // luego el usuario tendrá que hacer login.
    return this.http.post(`${this.apiUrl}/registrar`, usuario);
  }

  // 3. CERRAR SESIÓN
  logout(): void {
    // Destruimos la "pulsera VIP"
    localStorage.removeItem('token');
  }

  // 4. COMPROBAR SI ESTÁ LOGUEADO
  estaLogueado(): boolean {
    // Devuelve true si hay un token guardado, false si no lo hay
    return !!localStorage.getItem('token');
  }

  // 5. OBTENER EL TOKEN (Lo usaremos luego para el Interceptor)
  getToken(): string | null {
    return localStorage.getItem('token');
  }
}