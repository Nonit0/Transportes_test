using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TransportesBackend.Models;
using System.Linq;
using System.Collections.Generic;

namespace TransportesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConductoresController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        public ConductoresController(TransportesDbContext context) { _context = context; }

        // ======================= //
        // GET: api/Conductores    //
        // ======================= //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Conductor>>> GetConductores()
        {
            var conductores = await _context.Conductor.ToListAsync();
            return Ok(conductores);
        }

        // ======================= //
        // POST: api/Conductores   //
        // ======================= //
        [HttpPost]
        public async Task<ActionResult<Conductor>> PostConductor([FromBody] Conductor conductor)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Conductor.AnyAsync(c => c.Dni == conductor.Dni))
                return BadRequest(new { mensaje = "Ya existe un conductor con este DNI." });

            _context.Conductor.Add(conductor);
            await _context.SaveChangesAsync();

            return Ok(conductor);
        }

        // ========================== //
        // PUT: api/Conductores/{id}  //
        // ========================== //
        [HttpPut("{id}")]
        public async Task<IActionResult> PutConductor(string id, [FromBody] Conductor conductorActualizado)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var conductor = await _context.Conductor.FindAsync(id);
            if (conductor == null) return NotFound();

            if (await _context.Conductor.AnyAsync(c => c.Dni == conductorActualizado.Dni && c.Id != id))
                return BadRequest(new { mensaje = "Otro conductor ya tiene este DNI asignado." });

            conductor.Dni = conductorActualizado.Dni;
            conductor.Nombre = conductorActualizado.Nombre;
            conductor.Apellidos = conductorActualizado.Apellidos;
            conductor.Telefono = conductorActualizado.Telefono;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ============================ //
        // DELETE: api/Conductores/{id} //
        // ============================ //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConductor(string id)
        {
            var conductor = await _context.Conductor.FindAsync(id);
            if (conductor == null) return NotFound();

            // SOFT DELETE: Marcamos la baja lógica para mantener historial.
            conductor.DeletedAt = System.DateTime.UtcNow;
            _context.Conductor.Update(conductor);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
