import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl + '/auth';
  
  // BehaviorSubject que guarda el estado de si está o no logueado
  private loggedIn = new BehaviorSubject<boolean>(this.estaLogueado());

  // Observable que los componentes pueden escuchar
  isLoggedIn$ = this.loggedIn.asObservable();

  constructor(private http: HttpClient) { }

  // 1. LOGIN
  login(credenciales: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, credenciales).pipe(
      tap((respuesta: any) => {
        // Si C# nos devuelve un token, lo guardamos en el LocalStorage
        if (respuesta && respuesta.token) {
          localStorage.setItem('token', respuesta.token);
          this.loggedIn.next(true); // Notificamos a todos que estamos logueados
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
    this.loggedIn.next(false); // Notificamos que hemos cerrado sesión
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

  // 6. DECODIFICAR EL PAYLOAD DEL TOKEN
  private getPayload(): any {
    const token = this.getToken();
    if (!token) return null;
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      return JSON.parse(jsonPayload);
    } catch (e) {
      return null;
    }
  }

  // 7. OBTENER EL ROL DEL USUARIO
  getRol(): string | null {
    const payload = this.getPayload();
    return payload ? payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || payload["role"] : null;
  }

  // 8. OBTENER EL ID DEL CLIENTE (EMPRESA)
  getClienteId(): string | null {
    const payload = this.getPayload();
    return payload ? payload["ClienteId"] : null;
  }
}