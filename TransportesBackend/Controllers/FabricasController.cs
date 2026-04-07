using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TransportesBackend.Models;
using TransportesBackend.Services;

namespace TransportesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FabricasController : ControllerBase
    {
        private readonly IFabricaService _fabricaService;

        public FabricasController(IFabricaService fabricaService)
        {
            _fabricaService = fabricaService;
        }

        // =================== //
        // GET: api/Fabricas   //
        // =================== //
        [HttpGet]
        public ActionResult<IEnumerable<Fabrica>> GetFabricas()
        {
            var fabricas = _fabricaService.ObtenerTodas();
            return Ok(fabricas);
        }

        // ==================== //
        // POST: api/Fabricas   //
        // ==================== //
        [HttpPost]
        public ActionResult<Fabrica> CreateFabrica([FromBody] Fabrica fabrica)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!_fabricaService.ExisteDireccion(fabrica.DireccionId))
            {
                return BadRequest(new { mensaje = "La dirección seleccionada no existe en la base de datos." });
            }

            var fabricaCreada = _fabricaService.Crear(fabrica);
            return CreatedAtAction(nameof(GetFabricas), new { id = fabricaCreada.Id }, fabricaCreada);
        }

        // ========================== //
        // DELETE: api/Fabricas/{id}  //
        // ========================== //
        [HttpDelete("{id}")]
        public IActionResult DeleteFabrica(string id)
        {
            var eliminado = _fabricaService.Eliminar(id);
            if (!eliminado) return NotFound(new { mensaje = "La fábrica no existe o ya fue eliminada." });

            return NoContent();
        }

        // ======================== //
        // PUT: api/Fabricas/{id}   //
        // ======================== //
        [HttpPut("{id}")]
        public ActionResult<Fabrica> PutFabrica(string id, [FromBody] Fabrica fabricaActualizada)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var resultado = _fabricaService.Actualizar(id, fabricaActualizada);
            if (resultado == null) return NotFound(new { mensaje = "El registro no existe o ha sido eliminado." });

            return Ok(resultado);
        }
    }
}
