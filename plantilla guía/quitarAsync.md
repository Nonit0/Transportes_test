# 📘 Guía Detallada: Refactorización de Controladores en .NET 5
## De Mapeo Manual a Objetos Crudos y AutoMapper

Esta guía detalla cómo simplificar tus controladores eliminando el bloque `.Select(c => new DTO { ... })` (el mapeo manual o "creación de objetos abstractos"). El objetivo es mantener el código limpio y saber exactamente qué datos viajan entre tu base de datos y Angular.

---

## 🧠 Parte 1: Teoría y Conceptos Clave

Cuando Entity Framework lee la base de datos, extrae **Entidades**. Una entidad es el reflejo exacto de tu tabla (incluyendo relaciones complejas y campos sensibles). Un **DTO (Data Transfer Object)** es un objeto "recortado" que creamos para enviar solo lo que el frontend (Angular) necesita.

Al quitar el `.Select()` manual, tenemos dos caminos:

### Enfoque A: Enviar el Objeto "En Crudo" (La Entidad)
Consiste en escupir directamente lo que sale de la base de datos hacia Angular.
* **Pros:** Código de una sola línea. Perfecto para debugear y ver qué datos reales está trayendo tu backend.
* **Contras:** Si tu tabla `Camion` está relacionada con `Chofer`, y `Chofer` con `Camion`, el conversor a JSON entrará en un bucle infinito (Error 500: *Object cycle detected*). Además, envías datos pesados e innecesarios al cliente.

### Enfoque B: Usar AutoMapper (El estándar profesional)
Consiste en usar una librería que compara tu Entidad "Cruda" y tu "DTO". Si las propiedades se llaman igual (ej. `Matricula` y `Matricula`), copia los datos automáticamente por detrás.
* **Pros:** Mantienes la seguridad y ligereza del DTO, pero el código de tu controlador queda limpio y libre de asignaciones manuales repetitivas.
* **Contras:** Requiere instalar paquetes NuGet y una pequeña configuración inicial.



---

## 🛠️ Parte 2: Caso Práctico 1 - El Objeto "En Crudo"
Úsalo cuando necesites inspeccionar a fondo tu base de datos o para APIs internas muy simples. No requiere instalar nada.

### Implementación en el Controlador
Cambiamos el tipo de retorno a `Camion` (la clase generada por Scaffold) y quitamos el asincronismo y el `Select`.

```csharp
// Le ponemos la ruta "crudo" para que no choque con otros GET
// GET: api/Camiones/crudo
[HttpGet("crudo")]
public ActionResult<IEnumerable<Camion>> GetCamionesCrudos()
{
    // 1. Traemos la entidad directa de la DB de forma síncrona
    // Nota: El hilo se bloquea hasta que la DB responde.
    var camiones = _context.Camion
        .OrderByDescending(c => c.Activo)
        .ToList(); 

    // ▶️ PON TU BREAKPOINT AQUÍ
    return Ok(camiones); 
}
```

> **💡 Tip de Debugger:** Al detenerte en el `return`, revisa la variable `camiones` en la pestaña **Locals**. Verás la tabla completa, incluyendo IDs internos, claves foráneas y campos que Angular no necesita.

---

## 🚀 Parte 3: Caso Práctico 2 - Uso de AutoMapper
Esta es la forma definitiva de trabajar. Limpias el controlador sin perder el control de los datos.

### A. Dependencias (Instalación)
Abre la terminal integrada en VS Code y ejecuta:
```bash
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

### B. Configuración Global (`Startup.cs`)
Debemos enseñarle a .NET que AutoMapper existe para que pueda inyectarlo en los controladores. Ve al método `ConfigureServices`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Esto escanea todo tu proyecto buscando clases que hereden de "Profile"
    services.AddAutoMapper(typeof(Startup)); 
    
    services.AddControllers();
    // ... resto de tu configuración (CORS, DbContext, etc.)
}
```

### C. El Perfil de Traducción (`MappingProfile.cs`)
Crea un archivo nuevo (idealmente en una carpeta llamada `Profiles` o `Mappers`). Aquí defines las reglas de conversión.

```csharp
using AutoMapper;
using TransportesBackend.Models; // Tu Entidad
using TransportesBackend.DTOs;   // Tu DTO

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // La magia ocurre aquí: "Si ves un Camion, conviértelo a CamionDTO"
        CreateMap<Camion, CamionDTO>();
    }
}
```

### D. El Controlador Final
Así queda tu código de limpio. Inyectamos `IMapper` y lo usamos.

```csharp
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace TransportesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CamionesController : ControllerBase
    {
        private readonly TransportesContext _context;
        private readonly IMapper _mapper; // Declaramos el Mapper

        // Inyección de dependencias en el constructor
        public CamionesController(TransportesContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Camiones
        [HttpGet]
        public ActionResult<IEnumerable<CamionDTO>> GetCamiones()
        {
            // 1. Extraemos el objeto "Crudo" de la base de datos
            var camionesCrudos = _context.Camion
                .OrderByDescending(c => c.Activo)
                .ToList();

            // 2. AutoMapper hace el "Select" por nosotros automáticamente
            var listaDtos = _mapper.Map<IEnumerable<CamionDTO>>(camionesCrudos);

            // ▶️ PON TU BREAKPOINT AQUÍ
            return Ok(listaDtos);
        }
    }
}
```

---

## 🔍 Parte 4: Hoja de Trucos para el Debugger (VS Code)

Cuando tu código se detenga en un Breakpoint, abre la **Debug Console** (Consola de depuración) abajo y usa estos comandos para descubrir la verdad de tus peticiones:

| Comando / Acción | ¿Para qué sirve? | ¿Qué descubrirás? |
| :--- | :--- | :--- |
| Escribir: `Request.Headers` | Inspeccionar cabeceras HTTP | El `Content-Type`, el `Origin` (ej. localhost:4200) o Tokens de seguridad que envíe Angular. |
| Escribir: `Request.QueryString` | Inspeccionar la URL | Los parámetros que Angular haya puesto tras el `?` (ej. `?id=5`). |
| Escribir: `Request.ContentLength` | Revisar el Body | Si es `0`, el body está vacío (normal en GET). Si es `>0`, Angular está mandando un objeto oculto. |
| Inspeccionar: `camionesCrudos` | Ver la Entidad |  Verás datos masivos, fechas de registro y relaciones con otras tablas. |
| Inspeccionar: `listaDtos` | Ver el DTO | Verás un objeto ligero, seguro y listo para ser consumido por Angular. |

> **⚠️ Recordatorio de entorno:** Para que todo este flujo funcione sin interrupciones, recuerda que al levantar el backend (con `dotnet run` o F5), debes dejar esa terminal o proceso de depuración abierto y corriendo en segundo plano mientras pruebas desde Angular.