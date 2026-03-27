using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TransportesBackend.Models; 
using System.Linq;
using TransportesBackend.DTOs;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

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
        public async Task<ActionResult<IEnumerable<CamionDTO>>> GetCamiones()
        {
            // Traemos todos los camiones ordenados para que los activos salgan primero
            var camiones = await _context.Camion
                .OrderByDescending(c => c.Activo) 
                .Select(c => new CamionDTO
                {
                    Id = c.Id,
                    Matricula = c.Matricula,
                    CapacidadPeso = c.CapacidadPeso,
                    CapacidadVolumen = c.CapacidadVolumen,
                    Activo = c.Activo
                })
                .ToListAsync();

            return Ok(camiones);
        }

        // ==================== //
        // POST: api/Camiones   //
        // ==================== //
        [HttpPost]
        public async Task<ActionResult<CamionDTO>> CreateCamion([FromBody] CreateCamionDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Validar que la matrícula no exista ya (Evita error 500 de SQL por el UNIQUE)
            var matriculaExiste = await _context.Camion.AnyAsync(c => c.Matricula == dto.Matricula);
            if (matriculaExiste)
            {
                return BadRequest(new { mensaje = "Ya existe un camión registrado con esta matrícula." });
            }

            // 2. Mapear DTO a Entidad
            var nuevoCamion = new Camion
            {
                Matricula = dto.Matricula,
                CapacidadPeso = dto.CapacidadPeso,
                CapacidadVolumen = dto.CapacidadVolumen,
                Activo = dto.Activo
            };

            _context.Camion.Add(nuevoCamion);
            await _context.SaveChangesAsync();

            // 3. Devolver DTO
            var camionCreado = new CamionDTO
            {
                Id = nuevoCamion.Id,
                Matricula = nuevoCamion.Matricula,
                CapacidadPeso = nuevoCamion.CapacidadPeso,
                CapacidadVolumen = nuevoCamion.CapacidadVolumen,
                Activo = nuevoCamion.Activo
            };

            return CreatedAtAction(nameof(GetCamiones), new { id = camionCreado.Id }, camionCreado);
        }

        // ======================== //
        // PUT: api/Camiones/{id}   //
        // ======================== //
        [HttpPut("{id}")]
        public async Task<ActionResult<CamionDTO>> PutCamion(string id, [FromBody] UpdateCamionDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var camion = await _context.Camion.FindAsync(id);
            if (camion == null) return NotFound(new { mensaje = "El camión no existe." });

            // Comprobar que no estemos intentando poner una matrícula que ya tiene otro camión
            var matriculaDuplicada = await _context.Camion
                .AnyAsync(c => c.Matricula == dto.Matricula && c.Id != id);
                
            if (matriculaDuplicada)
            {
                return BadRequest(new { mensaje = "Otro camión ya utiliza esta matrícula." });
            }

            // Actualizar datos
            camion.Matricula = dto.Matricula;
            camion.CapacidadPeso = dto.CapacidadPeso;
            camion.CapacidadVolumen = dto.CapacidadVolumen;
            camion.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            var camionActualizadoDTO = new CamionDTO
            {
                Id = camion.Id,
                Matricula = camion.Matricula,
                CapacidadPeso = camion.CapacidadPeso,
                CapacidadVolumen = camion.CapacidadVolumen,
                Activo = camion.Activo
            };

            return Ok(camionActualizadoDTO);
        }

        // ========================== //
        // DELETE: api/Camiones/{id}  //
        // ========================== //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCamion(string id)
        {
            var camion = await _context.Camion.FindAsync(id);
            if (camion == null) return NotFound();

            // SOFT DELETE (Baja lógica): En lugar de destruirlo, lo marcamos como inactivo.
            // Así preservamos el historial en la tabla 'Carga'
            camion.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
