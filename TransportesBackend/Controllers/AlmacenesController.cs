using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TransportesBackend.Models; 
using System.Linq;
using System.Collections.Generic;
using System;

namespace TransportesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlmacenesController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        public AlmacenesController(TransportesDbContext context)
        {
            _context = context;
        }

        // =================== //
        // GET: api/Almacenes  //
        // =================== //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Almacen>>> GetAlmacenes() 
        {
            // Include carga la Dirección asociada en el JSON de salida
            var almacenes = await _context.Almacen
               .Include(a => a.Direccion)
               .ToListAsync();

            return Ok(almacenes);
        }

        // ==================== //
        // POST: api/Almacenes  //
        // ==================== //
        [HttpPost]
        public async Task<ActionResult<Almacen>> CreateAlmacen([FromBody] Almacen almacen)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificamos que la dirección elegida en el desplegable de Angular realmente existe
            var direccionExiste = await _context.Direccion.AnyAsync(d => d.Id == almacen.DireccionId);
            if (!direccionExiste)
            {
                return BadRequest(new { mensaje = "La dirección seleccionada no existe en la base de datos." });
            }

            _context.Almacen.Add(almacen);
            await _context.SaveChangesAsync();

            // Recargamos con el Include para devolver la Dirección completa
            var almacenCreado = await _context.Almacen
                .Include(a => a.Direccion)
                .FirstAsync(a => a.Id == almacen.Id);

            return CreatedAtAction(nameof(GetAlmacenes), new { id = almacenCreado.Id }, almacenCreado);
        }

        // ========================== //
        // DELETE: api/Almacenes/{id} //
        // ========================== //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlmacen(string id)
        {
            var almacen = await _context.Almacen.FindAsync(id);

            if (almacen == null)
            {
                return NotFound(new { mensaje = "El almacén no existe o ya fue eliminado." });
            }

            // SOFT DELETE
            almacen.DeletedAt = DateTime.UtcNow;
            _context.Almacen.Update(almacen);
            await _context.SaveChangesAsync();
            return NoContent(); 
        }

        // ======================== //
        // PUT: api/Almacenes/{id}  //
        // ======================== //
        [HttpPut("{id}")]
        public async Task<ActionResult<Almacen>> PutAlmacen(string id, [FromBody] Almacen almacenActualizado)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entidad = await _context.Almacen.FindAsync(id);

            if (entidad == null)
            {
                return NotFound(new { mensaje = "El registro no existe o ha sido eliminado." });
            }

            // Actualizar datos
            entidad.Nombre = almacenActualizado.Nombre;
            entidad.DireccionId = almacenActualizado.DireccionId;

            await _context.SaveChangesAsync();

            // Recargamos con Include para devolver la Dirección completa
            var resultado = await _context.Almacen
                .Include(a => a.Direccion)
                .FirstAsync(a => a.Id == entidad.Id);

            return Ok(resultado);
        }
    }
}