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
    public class CamionesController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        public CamionesController(TransportesDbContext context)
        {
            _context = context;
        }

        // =================== //
        // GET: api/Camiones   //
        // =================== //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Camion>>> GetCamiones()
        {
            // Traemos todos los camiones ordenados para que los activos salgan primero
            // IgnoreQueryFilters permite que se devuelvan los borrados logicamente (inactivos)
            var camiones = await _context.Camion
                .IgnoreQueryFilters()
                .OrderByDescending(c => c.Activo) 
                .ToListAsync();

            return Ok(camiones);
        }

        // ==================== //
        // POST: api/Camiones   //
        // ==================== //
        [HttpPost]
        public async Task<ActionResult<Camion>> CreateCamion([FromBody] Camion camion)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Validar que la matrícula no exista ya (Evita error 500 de SQL por el UNIQUE)
            // AnyAsync devuelve un booleano comprobando que exista
            var matriculaExiste = await _context.Camion.AnyAsync(c => c.Matricula == camion.Matricula);
            if (matriculaExiste)
            {
                return BadRequest(new { mensaje = "Ya existe un camión registrado con esta matrícula." });
            }

            _context.Camion.Add(camion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCamiones), new { id = camion.Id }, camion);
        }

        // ======================== //
        // PUT: api/Camiones/{id}   //
        // ======================== //
        [HttpPut("{id}")]
        public async Task<ActionResult> PutCamion(string id, [FromBody] Camion camionActualizado)
        {
            // 1. Seguridad básica: Que el ID de la URL sea el mismo que el del objeto
            if (id != camionActualizado.Id) return BadRequest();

            // 2. Validación de matrícula (la que ya tenías)
            var matriculaDuplicada = await _context.Camion
                .AnyAsync(c => c.Matricula == camionActualizado.Matricula && c.Id != id);
                
            if (matriculaDuplicada) return BadRequest("Matrícula duplicada");

            // 3. Le decimos a EF "Este objeto completo ha sido modificado, písalo todo"
            _context.Entry(camionActualizado).State = EntityState.Modified;

            // 4. Guardamos
            await _context.SaveChangesAsync();

            return NoContent(); // En los PUT es estándar devolver 204 NoContent
        }

        // ========================== //
        // DELETE: api/Camiones/{id}  //
        // ========================== //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCamion(string id)
        {
            var camion = await _context.Camion.FindAsync(id);
            if (camion == null) return NotFound();

            // SOFT DELETE: Marcamos la baja lógica para mantener historial.
            camion.Activo = false;
            camion.DeletedAt = System.DateTime.UtcNow;
            _context.Camion.Update(camion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
