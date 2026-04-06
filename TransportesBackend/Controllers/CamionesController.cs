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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Validar que la matrícula no exista ya (Evita error 500 de SQL por el UNIQUE)
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
        public async Task<ActionResult<Camion>> PutCamion(string id, [FromBody] Camion camionDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var camion = await _context.Camion.FindAsync(id);
            if (camion == null) return NotFound(new { mensaje = "El camión no existe." });

            // Comprobar que no estemos intentando poner una matrícula que ya tiene otro camión
            var matriculaDuplicada = await _context.Camion
                .AnyAsync(c => c.Matricula == camionDto.Matricula && c.Id != id);
                
            if (matriculaDuplicada)
            {
                return BadRequest(new { mensaje = "Otro camión ya utiliza esta matrícula." });
            }

            // Actualizar datos
            camion.Matricula = camionDto.Matricula;
            camion.CapacidadPeso = camionDto.CapacidadPeso;
            camion.CapacidadVolumen = camionDto.CapacidadVolumen;
            camion.Activo = camionDto.Activo;

            await _context.SaveChangesAsync();

            return Ok(camion);
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
