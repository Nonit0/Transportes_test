using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TransportesBackend.Models; // Tu namespace de modelos
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

        [HttpGet]
        // no necesita ser ni Task ni async
        public async Task<IEnumerable<AlmacenDTO>> GetAlmacenes() 
        {
            

                //forma correcta de hacerlo pues es mas fácil
                return await _context.Almacen
                   .Where(a => a.DeletedAt == null)
                   .Include(a => a.Direccion)
                   //.thenInclude(a => a.Ciudad)
                   .Select(a => new AlmacenDTO
                   {
                       Id = a.Id,
                       Nombre = a.Nombre,
                       DireccionCompleta = a.Direccion.Calle,
                       Ciudad = a.Direccion.Ciudad
                   })
                   .ToListAsync();

            //return Ok(almacenes);
        }

        [HttpPost]
        public async Task<ActionResult<AlmacenDTO>> CreateAlmacen([FromBody] CreateAlmacenDTO dto)
        {
            // 1. Validar que el DTO sea correcto (Required, etc.)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Devuelve errores de validación
            }

            // 2. Buscar si la dirección existe (Opcional: Crear dirección si no existe)
            // Por simplicidad, asumimos que la dirección ya existe en la tabla Direcciones
            var direccion = await _context.Direccion
                .FirstOrDefaultAsync(d => d.Ciudad == dto.Ciudad && d.Calle == dto.DireccionCompleta);

            if (direccion == null)
            {
                // Si no existe, la creamos (Opcional: Depende de tu lógica de negocio)
                direccion = new Direccion
                {
                    Calle = dto.DireccionCompleta,
                    Ciudad = dto.Ciudad,
                    Cp = dto.Cp,
                    Provincia = dto.Provincia,
                    Pais = dto.Pais
                };
                _context.Direccion.Add(direccion);
                await _context.SaveChangesAsync();
            }

            // 3. Crear el nuevo Almacén usando el DTO
            var nuevoAlmacen = new Almacen
            {
                Nombre = dto.Nombre,
                DireccionId = direccion.Id, // Asignamos la FK
                DeletedAt = null // Por defecto no está borrado
            };

            _context.Almacen.Add(nuevoAlmacen);
            await _context.SaveChangesAsync();

            // 4. Devolver el objeto creado con su ID generado por la base de datos
            // Usamos Select para devolverlo en formato DTO (limpio)
            var almacenCreado = await _context.Almacen
                .Where(a => a.Id == nuevoAlmacen.Id)
                .Include(a => a.Direccion)
                .Select(a => new AlmacenDTO
                {
                    Id = a.Id,
                    Nombre = a.Nombre,
                    DireccionCompleta = a.Direccion.Calle,
                    Ciudad = a.Direccion.Ciudad,
                    Cp = a.Direccion.Cp,
                    Provincia = a.Direccion.Provincia,
                    Pais = a.Direccion.Pais
                })
                .FirstAsync();

            // Devolvemos 201 Created (Estándar REST para creaciones exitosas)
            return CreatedAtAction(nameof(GetAlmacenes), new { id = almacenCreado.Id }, almacenCreado);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlmacen(string id)
        {
            // 1. Buscamos el almacén por su ID
            var almacen = await _context.Almacen.FindAsync(id);

            // 2. Si no existe (o ya está borrado), devolvemos 404 Not Found
            if (almacen == null || almacen.DeletedAt != null)
            {
                return NotFound(new { mensaje = "El almacén no existe o ya fue eliminado." });
            }

            // 3. SOFT DELETE: No usamos _context.Almacen.Remove()
            // Simplemente le ponemos la fecha y hora actual UTC
            almacen.DeletedAt = DateTime.UtcNow;

            // 4. Guardamos los cambios (Entity Framework hará un UPDATE, no un DELETE)
            await _context.SaveChangesAsync();

            // 5. Devolvemos 204 No Content (Estándar REST para borrados)
            return NoContent(); 
        }

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
                // .Include(e => e.<TablaRelacionada>) // Descomentar si vas a actualizar relaciones
                .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);

            if (entidad == null)
            {
                return NotFound(new { mensaje = "El registro no existe o ha sido eliminado." });
            }

            // 3. Mapear los nuevos valores del DTO a la Entidad
            entidad.Nombre = dto.Nombre;
            entidad.Direccion.Calle = dto.DireccionCompleta;
            entidad.Direccion.Ciudad = dto.Ciudad;
            entidad.Direccion.Cp = dto.Cp;
            entidad.Direccion.Provincia = dto.Provincia;
            entidad.Direccion.Pais = dto.Pais;
            
            // Si hay entidades relacionadas (Ej: Dirección), actualízalas aquí.
            // entidad.Relacion.<Campo> = dto.<DatoRelacionado>;

            // 4. Guardar cambios (Entity Framework detecta automáticamente qué columnas cambiaron)
            await _context.SaveChangesAsync();

            // 5. Mapear al DTO de salida para devolverlo al Frontend
            var entidadActualizadaDTO = new AlmacenDTO
            {
                Id = entidad.Id,
                Nombre = entidad.Nombre,
                DireccionCompleta = entidad.Direccion.Calle,
                Ciudad = entidad.Direccion.Ciudad,
                Cp = entidad.Direccion.Cp,
                Provincia = entidad.Direccion.Provincia,
                Pais = entidad.Direccion.Pais
            };

            // 6. Devolver 200 OK con el objeto fresco
            return Ok(entidadActualizadaDTO);
        }
    }
}