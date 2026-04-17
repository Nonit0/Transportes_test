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
    public class ConductoresController : ControllerBase
    {
        private readonly IConductorService _conductorService;

        public ConductoresController(IConductorService conductorService)
        {
            _conductorService = conductorService;
        }

        // ======================= //
        // GET: api/Conductores    //
        // ======================= //
        [HttpGet]
        public ActionResult<PaginatedResponse<Conductor>> GetConductores([FromQuery] int? page = null, [FromQuery] int? limit = null)
        {
            var conductores = _conductorService.ObtenerTodos(page, limit);
            return Ok(conductores);
        }

        // ======================= //
        // POST: api/Conductores   //
        // ======================= //
        [HttpPost]
        public ActionResult<Conductor> PostConductor([FromBody] Conductor conductor)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (_conductorService.ExisteDni(conductor.Dni))
            {
                return BadRequest(new { mensaje = "Ya existe un conductor con este DNI." });
            }

            var nuevoConductor = _conductorService.Crear(conductor);
            return Ok(nuevoConductor);
        }

        // ========================== //
        // PUT: api/Conductores/{id}  //
        // ========================== //
        [HttpPut("{id}")]
        public ActionResult<Conductor> PutConductor(string id, [FromBody] Conductor conductorActualizado)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (_conductorService.ExisteDni(conductorActualizado.Dni, id))
            {
                return BadRequest(new { mensaje = "Otro conductor ya tiene este DNI asignado." });
            }

            var actualizado = _conductorService.Actualizar(id, conductorActualizado);
            if (actualizado == null) return NotFound(new { mensaje = "El conductor no existe o fue eliminado." });

            return Ok(actualizado);
        }

        // ============================ //
        // DELETE: api/Conductores/{id} //
        // ============================ //
        [HttpDelete("{id}")]
        public IActionResult DeleteConductor(string id)
        {
            var eliminado = _conductorService.Eliminar(id);
            if (!eliminado) return NotFound();

            return NoContent();
        }
    }
}
