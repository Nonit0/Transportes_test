import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    constructor(private authService: AuthService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // Obtenemos el token desde el AuthService (que lo saca del LocalStorage)
        const token = this.authService.getToken();

        // Si tenemos un token, clonamos la petición y le añadimos el header 'Authorization'
        if (token) {
            request = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${token}`
                }
            });
        }

        return next.handle(request).pipe(
            map((event: HttpEvent<any>) => {
                if (event instanceof HttpResponse && event.body) {
                    const resolvedBody = this.resolveReferences(event.body);
                    return event.clone({ body: resolvedBody });
                }
                return event;
            })
        );
    }

    private resolveReferences(json: any): any {
        const byId = new Map<string, any>();
        
        // Fase 1: Recolectar y memorizar todos los punteros $id reales
        const buildMap = (obj: any) => {
            if (!obj || typeof obj !== 'object') return;
            if (obj.$id) byId.set(obj.$id, obj);
            
            for (const key of Object.keys(obj)) {
                buildMap(obj[key]);
            }
        };

        // Fase 2: Buscar cualquier $ref y restituirlo como apuntador real en la memoria JS
        const replaceRefs = (obj: any) => {
            if (!obj || typeof obj !== 'object') return;
            
            for (const key of Object.keys(obj)) {
                const child = obj[key];
                if (child && typeof child === 'object') {
                    if (child.$ref) {
                        obj[key] = byId.get(child.$ref);
                    } else {
                        replaceRefs(child);
                    }
                }
            }
        };

        buildMap(json);
        replaceRefs(json);
        
        return json;
    }
}
