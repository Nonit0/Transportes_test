# Plantilla: Angular - Enrutamiento (Router) y SPA

**Concepto:** Una *Single Page Application* (SPA) no recarga la página web al navegar. Angular intercepta el cambio de URL (ej. `/almacenes` a `/vehiculos`), destruye el componente actual y pinta el nuevo dinámicamente dentro de la etiqueta `<router-outlet>`.
**Propósito:** Separar la lógica en múltiples componentes (Principio de Responsabilidad Única) y crear un menú de navegación maestro (Navbar/Sidebar).

---

## 🏗️ Paso 1: Crear el Componente Específico
Nunca programamos la lógica de negocio en el `app.component.ts`. Ese archivo es solo el contenedor principal. Debemos crear un componente para cada "Pantalla" de nuestra aplicación.

Abre una terminal en la carpeta de tu frontend de Angular y ejecuta:
```bash
# ng g c = Angular Generate Component
ng g c almacenes
```
Esto creará una carpeta `src/app/almacenes/` con sus propios archivos `.ts`, `.html` y `.css`.

---

## ✂️ Paso 2: Migrar el Código (De App a tu nuevo Componente)
Debes "cortar" todo el HTML, CSS y TypeScript (variables y métodos) que tenías en `app.component` y "pegarlo" en tus nuevos archivos `almacenes.component`.

**Tu `app.component.ts` debe quedar completamente vacío y limpio:**
```typescript
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  // 🧹 Limpio. Aquí no hay llamadas HTTP ni listas.
}
```

---

## 🗺️ Paso 3: Configurar el Router en `app.module.ts`
Como al crear el proyecto le dijimos a Angular que no queríamos rutas por defecto (`--routing=false`), se lo añadimos manualmente. Es la forma más limpia de entender cómo funciona.

Abre `src/app/app.module.ts`:

```typescript
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
// 1. Importamos el módulo de Rutas de Angular
import { RouterModule, Routes } from '@angular/router'; 

import { AppComponent } from './app.component';
import { AlmacenesComponent } from './almacenes/almacenes.component'; // Auto-importado al generar

// 2. Definimos nuestro "Mapa" de URLs
const misRutas: Routes = [
  { path: 'almacenes', component: AlmacenesComponent },
  // { path: 'vehiculos', component: VehiculosComponent }, // Para el futuro
  
  // Ruta por defecto: Si el usuario entra a "localhost:4200/", lo redirigimos a almacenes
  { path: '', redirectTo: '/almacenes', pathMatch: 'full' },
  // Ruta comodín (Error 404): Si escribe algo raro en la URL, lo mandamos a almacenes
  { path: '**', redirectTo: '/almacenes' } 
];

@NgModule({
  declarations: [
    AppComponent,
    AlmacenesComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    // 3. Activamos el mapa de rutas en nuestra app
    RouterModule.forRoot(misRutas) 
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

---

## 🎨 Paso 4: El Esqueleto (Navbar y Router-Outlet)
Ahora que `app.component.html` está vacío, lo convertimos en nuestro "Layout" o plantilla maestra. Tendrá una barra superior fija y un hueco debajo donde las pantallas irán cambiando.

Abre `src/app/app.component.html`:

```html
<nav class="navbar">
  <div class="logo">🚚 TransportesApp</div>
  <ul class="nav-links">
    <li><a routerLink="/almacenes" routerLinkActive="activo">📦 Almacenes</a></li>
    <li><a routerLink="/vehiculos" routerLinkActive="activo">🚛 Vehículos (Próximamente)</a></li>
  </ul>
</nav>

<main class="contenido-principal">
  <router-outlet></router-outlet>
</main>
```

Y en `src/app/app.component.css` le damos un diseño profesional al menú:

```css
/* Diseño del Layout Base */
.navbar {
  background-color: #2c3e50;
  padding: 1rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: white;
  box-shadow: 0 4px 6px rgba(0,0,0,0.1);
}

.logo { font-size: 1.5rem; font-weight: bold; }

.nav-links {
  list-style: none;
  display: flex;
  gap: 20px;
  margin: 0;
  padding: 0;
}

.nav-links a {
  color: #bdc3c7;
  text-decoration: none;
  font-weight: bold;
  padding: 5px 10px;
  border-radius: 4px;
  transition: all 0.3s;
}

.nav-links a:hover { color: white; background-color: #34495e; }

/* La clase "activo" se aplica mágicamente gracias al routerLinkActive */
.nav-links a.activo { color: white; background-color: #007bff; }

.contenido-principal {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}
```
