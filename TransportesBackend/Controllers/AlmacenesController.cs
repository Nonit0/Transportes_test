using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TransportesBackend.Models; 
using System.Linq;
using TransportesBackend.DTOs;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;

namespace TransportesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlmacenesController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        // El motor de .NET inyecta el DbContext automáticamente aquí gracias al Startup.cs
        public AlmacenesController(TransportesDbContext context)
        {
            _context = context;
        }

        // =================== //
        // GET: api/Almacenes  //
        // =================== //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlmacenDTO>>> GetAlmacenes() 
        {
            //forma correcta de hacerlo pues es mas limpio
            var almacenes = await _context.Almacen
               .Where(a => a.DeletedAt == null)
               .Include(a => a.Direccion) // incluye la direccion
               //.thenInclude(a => a.Ciudad) // incluye la ciudad que proviene de direccion
               // la cantidad de thenInclude se definen en el DbContext
               .Select(a => new AlmacenDTO // lo que le enviamos al DTO
               {
                   Id = a.Id,
                   Nombre = a.Nombre,
                   DireccionCompleta = a.Direccion.Calle, // Dejamos esto como texto resumen para la tarjeta
                   DireccionId = a.DireccionId // VITAL: Necesario para el <select> en Angular
               })
               .ToListAsync();

            return Ok(almacenes);
        }

        // ==================== //
        // POST: api/Almacenes  //
        // ==================== //
        [HttpPost]
        public async Task<ActionResult<AlmacenDTO>> CreateAlmacen([FromBody] CreateAlmacenDTO dto)
        {
            // 1. Validar que el DTO sea correcto (Required, etc.)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Devuelve errores de validación
            }

            // 2. REGLA DE ARQUITECTURA: Verificamos que la dirección elegida en el desplegable de Angular realmente existe
            var direccionExiste = await _context.Direccion.AnyAsync(d => d.Id == dto.DireccionId); // CORREGIDO: Era DireccionId, no DireccionC
            if (!direccionExiste)
            {
                return BadRequest(new { mensaje = "La dirección seleccionada no existe en la base de datos." });
            }
            
            /* Ya no necesitamos comprobar de esta manera si la direccion existe porque ahora si no existe se crea aparte
            // 2. Buscar si la dirección existe (Opcional: Crear dirección si no existe)
            // Por simplicidad, asumimos que la dirección ya existe en la tabla Direcciones
            var direccion = await _context.Direccion
                .FirstOrDefaultAsync(d => d.Ciudad == dto.Ciudad && d.Calle == dto.DireccionCompleta);

            if (direccion == null)
            {
                direccion = new Direccion { ... };
                _context.Direccion.Add(direccion);
                await _context.SaveChangesAsync();
            }
            */

            // 3. Crear el nuevo Almacén usando el DTO
            var nuevoAlmacen = new Almacen
            {
                Nombre = dto.Nombre,
                DireccionId = dto.DireccionId, // CORREGIDO: Asignamos la FK del DTO directamente
                /* no necesitamos poner porque ahora se pone por defecto
                // DeletedAt = null // Por defecto no está borrado
                */
            };

            _context.Almacen.Add(nuevoAlmacen);
            await _context.SaveChangesAsync();

            // 4. Devolver el objeto creado con su ID generado por la base de datos
            var almacenCreado = await _context.Almacen
                .Where(a => a.Id == nuevoAlmacen.Id)
                .Include(a => a.Direccion)
                .Select(a => new AlmacenDTO
                {
                    Id = a.Id,
                    Nombre = a.Nombre,
                    DireccionCompleta = a.Direccion.Calle,
                    DireccionId = a.DireccionId
                    /* Ya no hacen falta pues estos pertenecen a direccion no a almacen
                    Ciudad = a.Direccion.Ciudad,
                    Cp = a.Direccion.Cp,
                    Provincia = a.Direccion.Provincia,
                    Pais = a.Direccion.Pais
                    */
                })
                .FirstAsync();

            // Devolvemos 201 Created (Estándar REST para creaciones exitosas)
            return CreatedAtAction(nameof(GetAlmacenes), new { id = almacenCreado.Id }, almacenCreado);
        }

        // ========================== //
        // DELETE: api/Almacenes/{id} //
        // ========================== //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlmacen(string id)
        {
            var almacen = await _context.Almacen.FindAsync(id);

            if (almacen == null || almacen.DeletedAt != null)
            {
                return NotFound(new { mensaje = "El almacén no existe o ya fue eliminado." });
            }

            // SOFT DELETE: No usamos _context.Almacen.Remove()
            almacen.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent(); 
        }

        // ======================== //
        // PUT: api/Almacenes/{id}  //
        // ======================== //
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        public async Task<ActionResult<AlmacenDTO>> PutAlmacen(string id, [FromBody] UpdateAlmacenDTO dto)
        {
            // 1. Validar DTO
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 2. Buscar la entidad existente (filtrando borrados lógicos)
            var entidad = await _context.Almacen
                .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);

            // 3. Si no existe, devolvemos 404
            if (entidad == null)
            {
                return NotFound(new { mensaje = "El registro no existe o ha sido eliminado." });
            }

            // 4. Mapear los nuevos valores del DTO a la Entidad
            entidad.Nombre = dto.Nombre;
            entidad.DireccionId = dto.DireccionId; // AQUI ESTÁ LA MAGIA: Solo cambiamos el enlace

            /* Ya no hacen falta pues estos pertenecen a direccion no a almacen
            entidad.Direccion.Calle = dto.DireccionCompleta; // BORRADO PARA NO MUTAR LA DIRECCIÓN
            entidad.Direccion.Ciudad = dto.Ciudad;
            entidad.Direccion.Cp = dto.Cp;
            entidad.Direccion.Provincia = dto.Provincia;
            entidad.Direccion.Pais = dto.Pais;
            */

            // 5. Guardar cambios
            await _context.SaveChangesAsync();

            // 6. Para devolver el objeto actualizado completo al frontend, volvemos a consultar a la BD
            var almacenActualizado = await _context.Almacen
                .Where(a => a.Id == entidad.Id)
                .Include(a => a.Direccion)
                .Select(a => new AlmacenDTO
                {
                    Id = a.Id,
                    Nombre = a.Nombre,
                    DireccionCompleta = a.Direccion.Calle,
                    DireccionId = a.DireccionId
                })
                .FirstAsync();

            // 7. Devolver 200 OK con el objeto fresco
            return Ok(almacenActualizado);
        }
    }
}