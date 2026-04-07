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
    public class FabricasController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        public FabricasController(TransportesDbContext context)
        {
            _context = context;
        }

        // =================== //
        // GET: api/Fabricas   //
        // =================== //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fabrica>>> GetFabricas()
        {
            var fabricas = await _context.Fabrica
               .Include(f => f.Direccion)
               .ToListAsync();

            return Ok(fabricas);
        }

        // ==================== //
        // POST: api/Fabricas   //
        // ==================== //
        [HttpPost]
        public async Task<ActionResult<Fabrica>> CreateFabrica([FromBody] Fabrica fabrica)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var direccionExiste = await _context.Direccion.AnyAsync(d => d.Id == fabrica.DireccionId);
            if (!direccionExiste)
            {
                return BadRequest(new { mensaje = "La dirección seleccionada no existe en la base de datos." });
            }

            _context.Fabrica.Add(fabrica);
            await _context.SaveChangesAsync();

            var fabricaCreada = await _context.Fabrica
                .Include(f => f.Direccion)
                .FirstAsync(f => f.Id == fabrica.Id);

            return CreatedAtAction(nameof(GetFabricas), new { id = fabricaCreada.Id }, fabricaCreada);
        }

        // ========================== //
        // DELETE: api/Fabricas/{id}  //
        // ========================== //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFabrica(string id)
        {
            var fabrica = await _context.Fabrica.FindAsync(id);

            if (fabrica == null)
            {
                return NotFound(new { mensaje = "La fábrica no existe o ya fue eliminada." });
            }

            // SOFT DELETE: Marcamos la baja lógica para mantener historial.
            fabrica.DeletedAt = DateTime.UtcNow;
            _context.Fabrica.Update(fabrica);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ======================== //
        // PUT: api/Fabricas/{id}   //
        // ======================== //
        [HttpPut("{id}")]
        public async Task<ActionResult<Fabrica>> PutFabrica(string id, [FromBody] Fabrica fabricaActualizada)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entidad = await _context.Fabrica.FindAsync(id);

            if (entidad == null)
            {
                return NotFound(new { mensaje = "El registro no existe o ha sido eliminado." });
            }

            entidad.Nombre = fabricaActualizada.Nombre;
            entidad.DireccionId = fabricaActualizada.DireccionId;

            await _context.SaveChangesAsync();

            var resultado = await _context.Fabrica
                .Include(f => f.Direccion)
                .FirstAsync(f => f.Id == entidad.Id);

            return Ok(resultado);
        }
    }
}
