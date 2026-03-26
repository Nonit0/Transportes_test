# Plantilla: Integración Frontend (Angular) y Configuración CORS

**Concepto:** CORS (Cross-Origin Resource Sharing) es un mecanismo de seguridad de los navegadores web. Bloquea las peticiones HTTP que un frontend (ej. `localhost:4200`) hace a un backend alojado en un puerto o dominio distinto (ej. `localhost:5001`), a menos que el backend dé permiso explícito.
**Propósito:** Abrir una vía de comunicación segura entre nuestra API en .NET y nuestro cliente en Angular para consumir nuestros DTOs.



---

## 🚧 Paso 1: Habilitar CORS en el Backend (.NET 5)
Debemos darle permiso explícito a la URL de nuestro frontend dentro de nuestro `Startup.cs`.

**1. Registrar la política en `ConfigureServices`:**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ... configuraciones previas (DbContext, etc) ...

    // 1. Definimos la política de CORS permitiendo el puerto 4200 de Angular
    services.AddCors(options =>
    {
        options.AddPolicy("PermitirAngular", builder =>
        {
            builder.WithOrigins("http://localhost:4200")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
    });

    services.AddControllers();
    // ... Swagger ...
}
```

**2. Activar el Middleware en `Configure`:**
**⚠️ ALERTA ARQUITECTÓNICA:** El orden de los middlewares en .NET es estricto. Si pones el CORS en el lugar equivocado, no funcionará.

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ... app.UseHttpsRedirection(); ...

    app.UseRouting();

    // 2. ¡VITAL! app.UseCors DEBE ir exactamente aquí:
    // DESPUÉS de UseRouting y ANTES de UseAuthorization
    app.UseCors("PermitirAngular"); 

    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}
```

---

## 🚀 Paso 2: Inicializar el Frontend (Angular 16)
Abrimos una nueva terminal fuera de la carpeta del backend y forzamos la creación del proyecto con la versión exacta de Angular que necesitamos usando `npx`.

```bash
npx @angular/cli@16 new TransportesFrontend --routing=false --style=css
cd TransportesFrontend
```

---

## 🪞 Paso 3: El Espejo del DTO (TypeScript)
Para mantener la robustez, el Frontend debe tener un "contrato" exacto que refleje el DTO que envía el Backend. 

Crea el archivo `src/app/almacen.dto.ts`:
```typescript
// Reflejo exacto de AlmacenDTO.cs
// No incluimos campos sensibles de la base de datos, solo lo que el Backend expone.
export interface AlmacenDTO {
  id: string;
  nombre: string;
  direccionCompleta: string;
  ciudad: string;
}
```

---

## 🔌 Paso 4: Consumo HTTP en Angular (MVP)

**1. Habilitar el módulo HTTP (`src/app/app.module.ts`):**
```typescript
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http'; // 1. Importar esto

import { AppComponent } from './app.component';

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    HttpClientModule // 2. Añadirlo a los imports
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

**2. Lógica del Componente (`src/app/app.component.ts`):**
```typescript
import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AlmacenDTO } from './almacen.dto'; 

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  almacenes: AlmacenDTO[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    // Apuntamos al puerto seguro HTTPS de nuestro backend .NET
    this.http.get<AlmacenDTO[]>('https://localhost:5001/api/Almacenes')
      .subscribe({
        next: (data) => this.almacenes = data,
        error: (err) => console.error('Error de API:', err)
      });
  }
}
```

**3. Pintar la Vista (`src/app/app.component.html`):**
Borra el contenido por defecto y usa este código mínimo para iterar los DTOs:
```html
<div style="padding: 20px; font-family: sans-serif;">
  <h1>📦 Panel de Almacenes</h1>
  <p *ngIf="almacenes.length === 0">Cargando...</p>
  <ul>
    <li *ngFor="let almacen of almacenes">
      <h2>{{ almacen.nombre }}</h2>
      <p>{{ almacen.direccionCompleta }} - {{ almacen.ciudad }}</p>
    </li>
  </ul>
</div>
```

**Arrancar el Frontend:**
```bash
npm start
```

---

## 🛑 Troubleshooting (Errores Comunes y Soluciones)

### 💥 Error: `CORS policy: No 'Access-Control-Allow-Origin' header is present`
**Síntoma:** Texto rojo en la consola del navegador. La API funciona en Swagger pero no en Angular.
**Diagnóstico:** El backend rechazó la petición del frontend por seguridad.
**Solución:** 1. Verifica que la URL en `builder.WithOrigins(...)` sea **exactamente** `http://localhost:4200` (sin barra final `/`).
2. Verifica que `app.UseCors("PermitirAngular");` esté situado estrictamente entre `UseRouting()` y `UseAuthorization()` en el `Startup.cs`.

### 💥 Error: `NET::ERR_CERT_AUTHORITY_INVALID` o `Http failure response for (unknown url): 0 Unknown Error`
**Síntoma:** Texto rojo en consola. Falla la conexión silenciosamente.
**Diagnóstico:** Angular está intentando acceder a `https://localhost:5001`, pero tu navegador no confía en el certificado SSL de desarrollo de .NET.
**Solución rápida:** Abre una nueva pestaña en tu navegador, navega manualmente a `https://localhost:5001/api/Almacenes`, haz clic en "Configuración avanzada" y luego en "Continuar a localhost (inseguro)". Vuelve a la pestaña de Angular y recarga.
