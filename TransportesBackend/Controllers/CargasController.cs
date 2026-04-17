using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using TransportesBackend.Models;
using TransportesBackend.Services;
using System.Collections.Generic;
using System;

namespace TransportesBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CargasController : ControllerBase
    {
        private readonly ICargaService _cargaService;

        public CargasController(ICargaService cargaService)
        {
            _cargaService = cargaService;
        }

        // =================== //
        // GET: api/Cargas     //
        // =================== //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Carga>>> GetCargas()
        {
            var cargas = await _cargaService.ObtenerTodas();
            return Ok(cargas);
        }

        // ==================== //
        // POST: api/Cargas     //
        // ==================== //
        [HttpPost]
        public async Task<ActionResult<Carga>> CreateCarga([FromBody] Carga cargaInput)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try 
            {
                var cargaCreada = await _cargaService.Crear(cargaInput);
                return CreatedAtAction(nameof(GetCargas), new { id = cargaCreada.Id }, cargaCreada);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // ========================== //
        // DELETE: api/Cargas/{id}    //
        // ========================== //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCarga(string id)
        {
            var eliminado = await _cargaService.Eliminar(id);
            if (!eliminado) return NotFound(new { mensaje = "La carga no existe o no tiene permisos para eliminarla." });

            return NoContent();
        }
    }
}
