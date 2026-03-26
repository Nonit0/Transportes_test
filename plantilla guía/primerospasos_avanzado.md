# Plantilla: Arquitectura Avanzada - Patrón DTO (Data Transfer Object)

**Concepto:** Un DTO es un objeto "tonto" (sin lógica de base de datos) que transporta datos entre procesos.
**Propósito:** Desacoplar la base de datos de la API. Evita fugas de datos sensibles (contraseñas, campos internos) y previene errores de serialización JSON (bucles infinitos de claves foráneas).

---

## 🏗️ Paso 1: Crear la estructura de carpetas
Nunca mezcles tus modelos de base de datos (`Models/`) con los modelos de tu API.
Crea una nueva carpeta en la raíz de tu proyecto llamada `DTOs`.

```bash
mkdir DTOs
```

## 🛡️ Paso 2: Diseñar el DTO de Salida (Response DTO)
Dentro de la carpeta `DTOs`, crea una clase que represente **exactamente** lo que quieres que vea el cliente (el frontend o la app móvil). 

Crea el archivo `DTOs/<NombreEntidad>DTO.cs` (por ejemplo, `AlmacenDTO.cs`).

```csharp
using System;

namespace <TuNamespace>.DTOs
{
    public class <NombreEntidad>DTO
    {
        // Solo incluimos los campos que el frontend realmente necesita ver.
        // Ocultamos el 'DeletedAt' y las propiedades de navegación de Entity Framework.
        
        public string Id { get; set; }
        public string Nombre { get; set; }
        
        // Podemos "Aplanar" (Flatten) las relaciones. 
        // En lugar de enviar un objeto 'Direccion' completo, enviamos solo el string que nos interesa.
        public string DireccionCompleta { get; set; } 
    }
}
```

## 🔄 Paso 3: Mapeo Manual en el Controlador (Entity -> DTO)
Ahora vamos a decirle a Entity Framework que no queremos que nos devuelva la entidad cruda, sino que la transforme a nuestro nuevo DTO antes de enviarla.

Abre tu `Controllers/<NombreEntidad>Controller.cs` y refactoriza el método `GET`.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using <TuNamespace>.Models;
using <TuNamespace>.DTOs; // Añadimos el using de nuestra nueva carpeta

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
        public async Task<ActionResult<IEnumerable<<NombreEntidad>DTO>>> GetAll()
        {
            // La magia ocurre en el .Select()
            // EF Core es lo bastante inteligente para leer esto y hacer un SELECT 
            // solo de estas columnas en SQL, mejorando radicalmente el rendimiento.

            var registros = await _context.<NombreDbSet> // Ej: _context.Almacens
                .Where(e => e.DeletedAt == null) // Ocultamos los borrados lógicamente
                .Select(e => new <NombreEntidad>DTO
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    // Aplanamos la relación de la clave foránea en tiempo real
                    DireccionCompleta = e.Direccion.Calle + ", " + e.Direccion.Ciudad
                })
                .ToListAsync();

            return Ok(registros);
        }
    }
}
```

## 🧹 Paso 4: Eliminar parches de JSON (Obligatorio para Frontend)
Si en las fases iniciales de tu proyecto añadiste `ReferenceHandler.Preserve` en tu `Startup.cs` para evitar bucles infinitos, **DEBES BORRARLO AHORA**.



**¿Por qué?**
Ese parche envuelve tus arrays limpios en un objeto de metadatos (`{ "$id": "1", "$values": [ ... ] }`). Angular, React o Vue esperan un array puro (`[ ... ]`) para poder iterarlo (ej. con `*ngFor`). Si les envías el objeto envuelto, el frontend crasheará con un error tipo `NG0900: Error trying to diff '[object Object]'`.

Como ahora usamos DTOs, el riesgo de bucles infinitos ha desaparecido. Ve a tu `Startup.cs` y deja tu registro de controladores completamente limpio:

```csharp
// ❌ INCORRECTO (Rompe el Frontend)
// services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve); 

// ✅ CORRECTO (Envía Arrays puros)
services.AddControllers(); 
```

---

## 🎯 Resultado Final en Swagger
Si ejecutas `dotnet run` y pruebas este endpoint en Swagger, notarás dos cosas de Nivel Senior:
1. El JSON de respuesta es muchísimo más pequeño, limpio y directo.
2. Si revisas los *logs* de tu consola, verás que la consulta SQL que EF Core lanzó a MySQL **ya no hace un `SELECT *`**, sino un `SELECT Id, Nombre, Calle, Ciudad...` reduciendo la carga de memoria de tu servidor a la mitad.
