# 🚚 TransportesApp - Sistema de Gestión Full-Stack

¡Bienvenido al repositorio de **TransportesApp**! Este proyecto es una aplicación de gestión logística empresarial construida desde cero, aplicando principios de arquitectura limpia, seguridad y experiencia de usuario fluida (SPA).

## 🛠️ Stack Tecnológico

**Backend (API REST):**
* **Framework:** .NET 5.0 (Legacy)
* **Base de Datos:** MySQL
* **ORM:** Entity Framework Core 5.0 (Enfoque *Database-First* / Scaffolding vía Pomelo)
* **Arquitectura:** Uso estricto de **DTOs** (Data Transfer Objects) para evitar fugas de datos sensibles, prevenir bucles infinitos de JSON y bloquear ataques de *Mass Assignment*.

**Frontend (SPA - Single Page Application):**
* **Framework:** Angular 16 *(Versión estricta)*
* **Estilos:** CSS3 puro con diseño responsivo basado en Flexbox y Grid.
* **Enrutamiento:** Angular Router (Layout maestro con barra de navegación + Vistas dinámicas).
* **Formularios:** *Template-Driven Forms* con validaciones reactivas (para evitar enviar datos erróneos al servidor).

---

## ✨ Características Principales (Módulo Almacenes)

* 📦 **CRUD Completo:** Creación, lectura, actualización y borrado de la entidad Almacenes.
* 🛡️ **Borrado Lógico (Soft Delete):** ¡Los datos nunca se destruyen! Se utiliza una marca temporal (`DeletedAt`) para ocultar los registros de la aplicación manteniendo la integridad e historial en la base de datos.
* ⚡ **UX Reactiva y Optimizada:** * El frontend no recarga la página. Utiliza manipulación de arrays en memoria (`.unshift()`, `.map()`, `.filter()`) para actualizar la interfaz al instante tras recibir el código de éxito HTTP (201, 200 o 204) del servidor.
  * Formulario "Inteligente": Un único componente que detecta si el usuario quiere crear o editar, adaptando sus botones y lógica automáticamente (Principio DRY).
* 🔌 **Seguridad CORS:** Comunicación cruzada entre el puerto `4200` (Angular) y el `5011` (.NET) explícitamente configurada y segura.

---

## 🚀 Cómo arrancar el proyecto en entorno local

Necesitarás dos terminales abiertas simultáneamente para correr el cliente y el servidor.

### 1. Levantar el Backend (.NET 5)
1. Asegúrate de tener tu servidor MySQL encendido (XAMPP, DBeaver, etc.) con la base de datos `transportes_db` disponible.
2. Abre una terminal en la carpeta `TransportesBackend`.
3. Ejecuta el servidor:
   ```bash
   dotnet run
   ```
4. La API y la documentación interactiva estarán disponibles en: `https://localhost:5011/swagger`

### 2. Levantar el Frontend (Angular 16)
1. Abre una segunda terminal en la carpeta `TransportesFrontend`.
2. Si es la primera vez que clonas el repositorio, instala las dependencias respetando la versión:
   ```bash
   npm install
   ```
   *(Nota: El proyecto fue inicializado estrictamente con `npx @angular/cli@16 new ...` para asegurar la compatibilidad).*
3. Inicia el servidor de desarrollo de Angular:
   ```bash
   npm start
   ```
4. Abre tu navegador y navega a la aplicación: `http://localhost:4200`

---

> **Nota de Desarrollo:** Este proyecto utiliza Angular Routing. Si navegas a una ruta inexistente, el sistema te redirigirá automáticamente de forma segura al panel principal de Almacenes.