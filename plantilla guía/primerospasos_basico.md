# Plantilla: Scaffolding EF Core 5 (Database-First) con MySQL

**Stack:** .NET 5 (Legacy) | EF Core 5.0 | Pomelo MySQL
**Propósito:** Generar modelos y DbContext a partir de una base de datos MySQL existente respetando restricciones arquitectónicas (UUIDs, Arcos Exclusivos).

---

## ⚠️ Paso 0: Prerrequisitos (CLI)
Instalar o fijar la herramienta global de EF Core a la rama 5.x para evitar conflictos con SDKs modernos.

```bash
dotnet tool install --global dotnet-ef --version 5.0.17
# Si ya está instalada: dotnet tool update --global dotnet-ef --version 5.0.17
```

## 🚀 Paso 1: Inicializar Proyecto Web API
Creamos la API forzando la estructura clásica (`Startup.cs` + `Program.cs`).

```bash
dotnet new webapi -n TransportesBackend -f net5.0
cd TransportesBackend
```

## 📦 Paso 2: Instalar Dependencias (Estrictas 5.x)
**Prohibido usar `dotnet add package` sin especificar versión.** Bajará versiones de .NET 6/8 y romperá la compilación.

```bash
# Motor core y herramientas de EF Core
dotnet add package Microsoft.EntityFrameworkCore.Design -v 5.0.17
dotnet add package Microsoft.EntityFrameworkCore.Tools -v 5.0.17

# Conector Pomelo optimizado para MySQL
dotnet add package Pomelo.EntityFrameworkCore.MySql -v 5.0.4
```

## 🏗️ Paso 3: Ejecutar Ingeniería Inversa (Scaffold)
Genera las entidades en la carpeta `/Models`. 

**Banderas (Flags) Arquitectónicas clave que usamos:**
* `--data-annotations`: Es obligatorio para mapear correctamente las restricciones de la BBDD (UUIDs, requeridos) directamente como atributos C#.
* `--no-pluralize`: **VITAL.** Evita que EF Core intente pluralizar tus tablas usando reglas gramaticales del inglés.
* `--force`: Sobrescribe los archivos de la carpeta `Models` si ya existían.

```bash
dotnet ef dbcontext scaffold 'Server=localhost;Database=transportes_db;User=root;Password=;' Pomelo.EntityFrameworkCore.MySql -o Models --context TransportesDbContext --data-annotations --no-pluralize --force
```

## 🧹 Paso 4: Limpieza Arquitectónica (Vital)
El comando anterior introduce un fallo de seguridad al hacer hardcoding de la conexión. Hay que limpiar esto manualmente aislando las credenciales.

**1. Limpiar el DbContext (`Models/TransportesDbContext.cs`)**
Borra o comenta el método `OnConfiguring`.

**2. Configurar `appsettings.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=transportes_db;user=root;password=[PASSWORD]"
  }
}
```

**3. Inyectar el Contexto en `Startup.cs`**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    string mySqlConnectionStr = Configuration.GetConnectionString("DefaultConnection");
    services.AddDbContext<TransportesDbContext>(options =>
        options.UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr)));

    services.AddControllers(); // SIN ReferenceHandler.Preserve
}
```

## 🛠️ Paso 5: Crear el Primer Controlador Genérico (MVP)
Crea un archivo `Controllers/<NombreEntidad>Controller.cs` para validar la conexión a la base de datos. 

**Regla de Arquitectura (Asincronía):** Todo acceso a base de datos debe ser asíncrono (`async Task`, `await`, `ToListAsync()`) para no bloquear el hilo principal del servidor mientras MySQL responde.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using System.Collections.Generic;
using System.Threading.Tasks;
using <TuNamespace>.Models;

namespace <TuNamespace>.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class <NombreEntidad>Controller : ControllerBase
    {
        private readonly <TuDbContext> _context;

        public <NombreEntidad>Controller(<TuDbContext> context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<<NombreEntidad>>>> Get()
        {
            var registros = await _context.<NombreDbSet>.ToListAsync();
            return Ok(registros);
        }
    }
}
```

## 🚀 Paso 6: Arrancar el Servidor
```bash
dotnet run
```
Navega a `https://localhost:5001/swagger`.
