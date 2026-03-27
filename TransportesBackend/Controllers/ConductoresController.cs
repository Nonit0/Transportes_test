using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TransportesBackend.Models;
using System.Linq;
using TransportesBackend.DTOs;
using System.Collections.Generic;

namespace TransportesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConductoresController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        public ConductoresController(TransportesDbContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConductorDTO>>> GetConductores()
        {
            var conductores = await _context.Conductor
                .Select(c => new ConductorDTO {
                    Id = c.Id, Dni = c.Dni, Nombre = c.Nombre, Apellidos = c.Apellidos, Telefono = c.Telefono
                }).ToListAsync();
            return Ok(conductores);
        }

        [HttpPost]
        public async Task<ActionResult<ConductorDTO>> PostConductor([FromBody] CreateConductorDTO dto)
        {
            if (await _context.Conductor.AnyAsync(c => c.Dni == dto.Dni))
                return BadRequest(new { mensaje = "Ya existe un conductor con este DNI." });

            var conductor = new Conductor {
                Dni = dto.Dni, Nombre = dto.Nombre, Apellidos = dto.Apellidos, Telefono = dto.Telefono
            };

            _context.Conductor.Add(conductor);
            await _context.SaveChangesAsync();

            return Ok(new ConductorDTO { Id = conductor.Id, Dni = conductor.Dni, Nombre = conductor.Nombre, Apellidos = conductor.Apellidos, Telefono = conductor.Telefono });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutConductor(string id, [FromBody] UpdateConductorDTO dto)
        {
            var conductor = await _context.Conductor.FindAsync(id);
            if (conductor == null) return NotFound();

            if (await _context.Conductor.AnyAsync(c => c.Dni == dto.Dni && c.Id != id))
                return BadRequest(new { mensaje = "Otro conductor ya tiene este DNI asignado." });

            conductor.Dni = dto.Dni;
            conductor.Nombre = dto.Nombre;
            conductor.Apellidos = dto.Apellidos;
            conductor.Telefono = dto.Telefono;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConductor(string id)
        {
            var conductor = await _context.Conductor.FindAsync(id);
            if (conductor == null) return NotFound();

            _context.Conductor.Remove(conductor);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
