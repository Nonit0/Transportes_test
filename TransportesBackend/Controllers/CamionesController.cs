using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using TransportesBackend.Models;
using TransportesBackend.Services;

namespace TransportesBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CamionesController : ControllerBase
    {
        private readonly ICamionService _camionService;

        public CamionesController(ICamionService camionService)
        {
            _camionService = camionService;
        }

        // =================== //
        // GET: api/Camiones   //
        // =================== //
        [HttpGet]
        public ActionResult<IEnumerable<Camion>> GetCamiones()
        {
            var camiones = _camionService.ObtenerTodos();
            return Ok(camiones);
        }

        // ==================== //
        // POST: api/Camiones   //
        // ==================== //
        [HttpPost]
        public ActionResult<Camion> CreateCamion([FromBody] Camion camion)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (_camionService.ExisteMatricula(camion.Matricula))
            {
                return BadRequest(new { mensaje = "Ya existe un camión registrado con esta matrícula." });
            }

            var nuevoCamion = _camionService.Crear(camion);
            return CreatedAtAction(nameof(GetCamiones), new { id = nuevoCamion.Id }, nuevoCamion);
        }

        // ======================== //
        // PUT: api/Camiones/{id}   //
        // ======================== //
        [HttpPut("{id}")]
        public ActionResult<Camion> PutCamion(string id, [FromBody] Camion camionActualizado)
        {
            System.Console.WriteLine($"DEBUG PUT ID: {id}");
            System.Console.WriteLine($"DEBUG JSON MATRICULA: {camionActualizado?.Matricula}");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (_camionService.ExisteMatricula(camionActualizado.Matricula, id))
            {
                return BadRequest(new { mensaje = "Ya existe un camión registrado con esta matrícula." });
            }

            var actualizado = _camionService.Actualizar(id, camionActualizado);
            if (actualizado == null) return NotFound(new { mensaje = "El camión no existe o fue eliminado." });

            return Ok(actualizado);
        }

        // ========================== //
        // DELETE: api/Camiones/{id}  //
        // ========================== //
        [HttpDelete("{id}")]
        public IActionResult DeleteCamion(string id)
        {
            var eliminado = _camionService.Eliminar(id);
            if (!eliminado) return NotFound();

            return NoContent();
        }
    }
}
