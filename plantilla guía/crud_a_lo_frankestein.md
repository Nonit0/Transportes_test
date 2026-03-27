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


# Plantilla: CRUD - Método POST (Crear) Avanzado Full-Stack

**Concepto:** Para insertar datos, usamos un **Input DTO** (evita *Mass Assignment*). Al finalizar, el estándar REST dicta que debemos devolver un código **201 Created**, la URL donde consultar el recurso, y el objeto recién creado formateado como un **Output DTO**.

---

## 🛡️ Paso 1: Crear el Input DTO (`CreateDTO`) en .NET
En la carpeta `DTOs/`, crea `Create<NombreEntidad>DTO.cs`.
**Regla:** NUNCA lleva `Id` ni campos de auditoría (`DeletedAt`). Solo lo que el usuario envía en el formulario.

> **⚠️ NOTA DE ARQUITECTURA (Validaciones de BBDD):** > Si tu base de datos tiene columnas marcadas como `NOT NULL` (ej. Código Postal, Provincia), **debes** incluirlas en tu DTO con la etiqueta `[Required]`. Si no lo haces, Entity Framework intentará guardar un `null` y la base de datos lanzará un error 500.

```csharp
using System.ComponentModel.DataAnnotations;

namespace <TuNamespace>.DTOs
{
    public class Create<NombreEntidad>DTO
    {
        [Required(ErrorMessage = "El campo es obligatorio")]
        public string <Propiedad1> { get; set; } // Ej: Nombre
        
        // Si hay relación con otra tabla, pedimos los datos necesarios para enlazarla
        [Required]
        public string <DatoRelacionado> { get; set; } // Ej: Ciudad o DireccionId
    }
}
```

---

## 📥 Paso 2: El Método POST en el Controlador (Nivel Senior)
Abre `Controllers/<NombreEntidad>Controller.cs`. 
Este método es inteligente: valida, maneja claves foráneas si es necesario, guarda, formatea la salida y devuelve un 201.

```csharp
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)] // Para que Swagger lo documente en verde
        public async Task<ActionResult<<NombreEntidad>DTO>> Post<NombreEntidad>([FromBody] Create<NombreEntidad>DTO dto)
        {
            // 1. Validar el DTO de entrada
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 2. [OPCIONAL] Inteligencia Relacional (Claves Foráneas)
            /*
            var entidadRelacionada = await _context.<TablaRelacionada>
                .FirstOrDefaultAsync(r => r.Campo == dto.<DatoRelacionado>);

            if (entidadRelacionada == null)
            {
                entidadRelacionada = new <ClaseRelacionada> { Campo = dto.<DatoRelacionado> };
                _context.<TablaRelacionada>.Add(entidadRelacionada);
                await _context.SaveChangesAsync();
            }
            */

            // 3. Mapear el DTO a la Entidad de Base de Datos
            var nuevaEntidad = new <NombreEntidad>
            {
                <Columna1> = dto.<Propiedad1>,
                // <ColumnaFK_Id> = entidadRelacionada.Id
            };

            // 4. Guardar en Base de Datos (MySQL genera el UUID automáticamente si está configurado)
            _context.<NombreDbSet>.Add(nuevaEntidad);
            await _context.SaveChangesAsync();

            // 5. Re-consultar y Mapear al Output DTO
            var entidadCreadaDTO = await _context.<NombreDbSet>
                .Where(e => e.Id == nuevaEntidad.Id)
                .Select(e => new <NombreEntidad>DTO
                {
                    Id = e.Id,
                    <Propiedad1> = e.<Columna1>,
                })
                .FirstAsync();

            // 6. Devolver 201 Created (Estándar REST)
            return CreatedAtAction(nameof(Get<NombreEntidad>s), new { id = entidadCreadaDTO.Id }, entidadCreadaDTO);
        }
```

---

## 🌐 Paso 3: Integración Frontend (Angular)

Para enviar datos desde Angular a .NET, necesitamos tres cosas:

**1. Habilitar Formularios en `app.module.ts`:**
```typescript
import { FormsModule } from '@angular/forms';
// ... añadir FormsModule al array de imports [...]
```

**2. Crear el DTO Espejo en TypeScript (`<entidad>.dto.ts`):**
```typescript
export interface Create<NombreEntidad>DTO {
  propiedad1: string;
  datoRelacionado: string;
}
```

**3. Lógica del Componente (La magia del `unshift`):**
```typescript
  nuevoElemento: Create<NombreEntidad>DTO = { propiedad1: '', datoRelacionado: '' };

  crearElemento() {
    this.http.post<<NombreEntidad>DTO>('https://localhost:5001/api/<NombreEntidad>', this.nuevoElemento)
      .subscribe({
        next: (creado) => {
          // UX: Usamos unshift() para meter el nuevo registro ARRIBA del todo en la lista visual
          // (push() lo metería al final, obligando al usuario a hacer scroll)
          this.listaElementos.unshift(creado); 
          
          // Limpiar formulario
          this.nuevoElemento = { propiedad1: '', datoRelacionado: '' };
        },
        error: (err) => console.error(err)
      });
  }
```

**4. Vista HTML (`<entidad>.component.html`) con Validaciones Reactivas:**
Para evitar enviar datos basura al backend, usamos Template-Driven Forms.
```html
<form #miForm="ngForm" (ngSubmit)="crearElemento()">
  
  <input type="text" name="prop1" [(ngModel)]="nuevoElemento.propiedad1" required>

  <div class="input-con-error">
    <input type="text" name="cp" [(ngModel)]="nuevoElemento.cp" required pattern="^[0-9]+$" #cpInput="ngModel">
    
    <small class="texto-error" *ngIf="cpInput.errors?.['pattern'] && cpInput.touched">
      ❌ Solo se admiten números.
    </small>
  </div>

  <button type="submit" [disabled]="miForm.invalid" [class.btn-deshabilitado]="miForm.invalid">
    Guardar
  </button>
</form>
```

---

## 🚀 ¿Por qué este enfoque es Superior?
1. **Evita llamadas extra en Angular:** Al devolver `entidadCreadaDTO`, el frontend hace `unshift()` a su array local y pinta el registro nuevo instantáneamente sin tener que lanzar un nuevo `GET`.
2. **Cumple el protocolo REST:** `CreatedAtAction` devuelve un código **201** y añade la cabecera `Location`.
3. **Protege la integridad de la BD:** Evita errores de Claves Foráneas comprobando o creando las dependencias antes del insert principal.
4. **Protege al Usuario (UX):** Las validaciones reactivas en el HTML guían al usuario y ahorran peticiones fallidas al servidor.

# Plantilla: CRUD - Método DELETE (Borrado Lógico / Soft Delete) Full-Stack

**Concepto:** En aplicaciones empresariales **nunca se hace un Hard Delete** (borrar físicamente la fila con `DELETE FROM...`). En su lugar, usamos **Soft Delete**: actualizamos una columna `DeletedAt` con la fecha y hora actual. El registro desaparece de la aplicación (porque en los `GET` filtramos los que tengan `DeletedAt == null`), pero se conserva en la base de datos por motivos de auditoría o recuperación.

---

## 🛡️ Paso 1: El Backend (Controlador .NET)

Abre `Controllers/<NombreEntidad>Controller.cs`.
**Regla:** El estándar REST para un borrado exitoso dicta que debemos devolver un código HTTP **204 No Content** (que significa: "La acción se completó, pero no tengo datos que devolverte").

> **⚠️ NOTA DE ARQUITECTURA (using System):** > Para usar `DateTime.UtcNow`, asegúrate de tener `using System;` en la parte superior de tu archivo. Si prefieres no poner el using, puedes escribir `System.DateTime.UtcNow` directamente.

```csharp
using System; // Vital para usar DateTime
using Microsoft.AspNetCore.Mvc;
// ... otros usings ...

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete<NombreEntidad>(string id)
        {
            // 1. Buscamos la entidad en la base de datos
            var entidad = await _context.<NombreDbSet>.FindAsync(id);

            // 2. Si no existe (o ya está borrada), devolvemos 404 Not Found
            if (entidad == null || entidad.DeletedAt != null)
            {
                return NotFound(new { mensaje = "El registro no existe o ya fue eliminado." });
            }

            // 3. LA MAGIA DEL SOFT DELETE: Actualizamos la fecha en lugar de borrar
            // NUNCA hacer: _context.<NombreDbSet>.Remove(entidad);
            entidad.DeletedAt = DateTime.UtcNow;

            // 4. Guardamos los cambios (Entity Framework hará un UPDATE, no un DELETE)
            await _context.SaveChangesAsync();

            // 5. Devolvemos 204 No Content
            return NoContent(); 
        }
```

---

## 🌐 Paso 2: Integración Frontend (Angular)

En el frontend, la clave es actualizar la Interfaz de Usuario (UI) inmediatamente después del borrado exitoso, filtrando el array local para no tener que hacer una recarga completa desde el servidor.

**1. Lógica del Componente (`<entidad>.component.ts`):**
```typescript
  eliminarElemento(id: string, nombre: string) {
    // UX: Siempre pedir confirmación antes de una acción destructiva
    const confirmacion = confirm(`¿Estás seguro de que quieres eliminar "${nombre}"?`);
    
    if (confirmacion) {
      this.http.delete(`https://localhost:5001/api/<NombreEntidad>s/${id}`)
        .subscribe({
          next: () => {
            console.log(`Registro ${id} eliminado con éxito.`);
            
            // Magia Frontend: Filtramos el array local para quitar la tarjeta al instante.
            // Conservamos todos los elementos cuyo ID sea diferente al que acabamos de borrar.
            this.listaElementos = this.listaElementos.filter(e => e.id !== id);
          },
          error: (err) => {
            console.error('Error al eliminar', err);
            alert('Hubo un problema al intentar eliminar el registro.');
          }
        });
    }
  }
```

**2. Vista HTML (`<entidad>.component.html`):**
```html
  <div *ngFor="let elemento of listaElementos" class="tarjeta">
    <div class="tarjeta-header">
      <h2>{{ elemento.nombre }}</h2>
      
      <button class="btn-eliminar" (click)="eliminarElemento(elemento.id, elemento.nombre)" title="Eliminar">
        🗑️
      </button>
    </div>
    
    </div>
```

---

## 🚀 ¿Por qué este enfoque es Superior?
1. **Seguridad de Datos:** Si un usuario borra algo por error, un administrador (o tú desde la BBDD) puede recuperarlo simplemente poniendo `DeletedAt = NULL`.
2. **Eficiencia Frontend:** Usar `.filter()` evita hacer un nuevo `GET` masivo a la API para refrescar la lista. Ahorramos ancho de banda y tiempo del servidor.
3. **UX (Experiencia de Usuario):** El `confirm()` previene borrados accidentales y el `204 No Content` asegura que la red no envíe payloads de respuesta innecesarios.

# Plantilla: CRUD - Método PUT (Actualizar) Avanzado Full-Stack

**Concepto:** El método `PUT` se usa para actualizar un registro existente. El estándar REST dicta que el ID viaja en la URL (`/api/entidad/{id}`) y los datos nuevos viajan en el cuerpo (Body) usando un **UpdateDTO**. Al finalizar, devolvemos un **200 OK** con el objeto actualizado para que el frontend refresque su vista inmediatamente.

---

## 🛡️ Paso 1: Crear el Input DTO (`UpdateDTO`) en .NET
En la carpeta `DTOs/`, crea `Update<NombreEntidad>DTO.cs`.
**Nota:** A menudo es idéntico al `CreateDTO`. Sin embargo, a nivel de arquitectura, se separan en dos clases distintas porque en el futuro los requisitos cambian (ej. al crear un Usuario exiges un "Password", pero al actualizarlo no).

```csharp
using System.ComponentModel.DataAnnotations;

namespace <TuNamespace>.DTOs
{
    public class Update<NombreEntidad>DTO
    {
        [Required(ErrorMessage = "El campo es obligatorio")]
        public string <Propiedad1> { get; set; } 
        
        [Required]
        public string <DatoRelacionado> { get; set; } 
    }
}
```

---

## 📥 Paso 2: El Método PUT en el Controlador (Nivel Senior)
Abre `Controllers/<NombreEntidad>Controller.cs`. 

```csharp
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<<NombreEntidad>DTO>> Put<NombreEntidad>(string id, [FromBody] Update<NombreEntidad>DTO dto)
        {
            // 1. Validar DTO
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 2. Buscar la entidad existente (filtrando borrados lógicos)
            var entidad = await _context.<NombreDbSet>
                // .Include(e => e.<TablaRelacionada>) // Descomentar si vas a actualizar relaciones
                .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);

            if (entidad == null)
            {
                return NotFound(new { mensaje = "El registro no existe o ha sido eliminado." });
            }

            // 3. Mapear los nuevos valores del DTO a la Entidad
            entidad.<Columna1> = dto.<Propiedad1>;
            
            // Si hay entidades relacionadas (Ej: Dirección), actualízalas aquí.
            // entidad.Relacion.<Campo> = dto.<DatoRelacionado>;

            // 4. Guardar cambios (Entity Framework detecta automáticamente qué columnas cambiaron)
            await _context.SaveChangesAsync();

            // 5. Mapear al DTO de salida para devolverlo al Frontend
            var entidadActualizadaDTO = new <NombreEntidad>DTO
            {
                Id = entidad.Id,
                <Propiedad1> = entidad.<Columna1>,
                // <PropiedadExtra> = entidad.<TablaRelacionada>.<Campo>
            };

            // 6. Devolver 200 OK con el objeto fresco
            return Ok(entidadActualizadaDTO);
        }
```

---

## 🌐 Paso 3: Integración Frontend (Angular)

Para conseguir la mejor UX, usamos el **mismo formulario** para Crear y para Editar. Simplemente guardamos el ID del elemento que estamos editando en una variable.

**1. Lógica del Componente (`<entidad>.component.ts`):**
```typescript
  // Variable para saber si estamos en modo "Crear" (null) o "Editar" (contiene un ID)
  idEdicion: string | null = null;
  formularioDTO: Update<NombreEntidad>DTO = { propiedad1: '', datoRelacionado: '' };

  // Método que se lanza al pulsar el botón ✏️ en la tarjeta
  cargarParaEdicion(elemento: <NombreEntidad>DTO) {
    this.idEdicion = elemento.id;
    // Rellenamos el formulario con los datos actuales
    this.formularioDTO = { 
      propiedad1: elemento.propiedad1, 
      datoRelacionado: elemento.datoRelacionado 
    };
    
    // UX: Hacemos scroll suave hacia arriba donde está el formulario
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // El método del formulario ahora decide si hace POST o PUT
  guardarElemento() {
    if (this.idEdicion) {
      // 🔵 MODO EDICIÓN (PUT)
      this.http.put<<NombreEntidad>DTO>(`https://localhost:5001/api/<NombreEntidad>s/${this.idEdicion}`, this.formularioDTO)
        .subscribe({
          next: (actualizado) => {
            // Magia Frontend: Buscamos el elemento en el array y lo reemplazamos
            // usando .map() para mantener la inmutabilidad y reactividad de Angular
            this.listaElementos = this.listaElementos.map(e => e.id === actualizado.id ? actualizado : e);
            this.resetearFormulario();
          },
          error: (err) => console.error(err)
        });
    } else {
      // 🟢 MODO CREACIÓN (POST) - (El código que ya teníamos)
      this.http.post<<NombreEntidad>DTO>('https://localhost:5001/api/<NombreEntidad>s', this.formularioDTO)
        .subscribe({
          next: (creado) => {
            this.listaElementos.unshift(creado); 
            this.resetearFormulario();
          },
          error: (err) => console.error(err)
        });
    }
  }

  // Método auxiliar para limpiar el estado
  resetearFormulario() {
    this.idEdicion = null;
    this.formularioDTO = { propiedad1: '', datoRelacionado: '' };
  }
```

**2. Vista HTML (`<entidad>.component.html`):**
```html
<h2>{{ idEdicion ? '✏️ Editar Registro' : '➕ Nuevo Registro' }}</h2>

<form #miForm="ngForm" (ngSubmit)="guardarElemento()">
  <div class="botones-form">
    <button type="submit" [disabled]="miForm.invalid">
      {{ idEdicion ? 'Actualizar' : 'Guardar' }}
    </button>
    
    <button type="button" *ngIf="idEdicion" (click)="resetearFormulario()">Cancelar</button>
  </div>
</form>

<div *ngFor="let elemento of listaElementos" class="tarjeta">
  <div class="tarjeta-header">
    <h2>{{ elemento.nombre }}</h2>
    <div>
      <button class="btn-editar" (click)="cargarParaEdicion(elemento)" title="Editar">✏️</button>
      <button class="btn-eliminar" (click)="eliminarElemento(elemento.id, elemento.nombre)">🗑️</button>
    </div>
  </div>
</div>
```

---

## 🚀 ¿Por qué este enfoque es Superior?
1. **Reutilización:** Usamos el mismo formulario HTML para dos operaciones, reduciendo el código duplicado a la mitad.
2. **Inmutabilidad en el Frontend:** Usar `.map()` para actualizar el array local asegura que Angular detecte el cambio y repinte la tarjeta al instante sin tener que hacer otro `GET`.
3. **Optimización de UX:** Subir el scroll automáticamente cuando el usuario le da a editar ( `window.scrollTo` ) evita que se pierda en listas muy largas.
