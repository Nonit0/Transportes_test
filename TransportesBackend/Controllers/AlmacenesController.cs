using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using TransportesBackend.Models;
using TransportesBackend.Services;
using System.Threading.Tasks;

namespace TransportesBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AlmacenesController : ControllerBase
    {
        // El controlador ya NO conoce el DbContext directamente
        // Solo conoce el servicio a través de su interfaz
        private readonly IAlmacenService _almacenService;

        // Inyección de dependencias: .NET nos pasa el servicio automáticamente
        public AlmacenesController(IAlmacenService almacenService)
        {
            _almacenService = almacenService;
        }

        // =================== //
        // GET: api/Almacenes  //
        // =================== //
        [HttpGet]
        public ActionResult<PaginatedResponse<Almacen>> GetAlmacenes([FromQuery] int? page = null, [FromQuery] int? limit = null)
        {
            var almacenes = _almacenService.ObtenerTodos(page, limit);
            return Ok(almacenes);
        }

        // ==================== //
        // POST: api/Almacenes  //
        // ==================== //
        [HttpPost]
        public ActionResult<Almacen> CreateAlmacen([FromBody] Almacen almacen)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Verificamos que la dirección elegida en el desplegable de Angular realmente existe
            if (!_almacenService.ExisteDireccion(almacen.DireccionId))
            {
                return BadRequest(new { mensaje = "La dirección seleccionada no existe en la base de datos." });
            }

            var almacenCreado = _almacenService.Crear(almacen);
            return CreatedAtAction(nameof(GetAlmacenes), new { id = almacenCreado.Id }, almacenCreado);
        }

        // ========================== //
        // DELETE: api/Almacenes/{id} //
        // ========================== //
        [HttpDelete("{id}")]
        public IActionResult DeleteAlmacen(string id)
        {
            var eliminado = _almacenService.Eliminar(id);

            if (!eliminado)
            {
                return NotFound(new { mensaje = "El almacén no existe o ya fue eliminado." });
            }

            return NoContent();
        }

        // ======================== //
        // PUT: api/Almacenes/{id}  //
        // ======================== //
        [HttpPut("{id}")]
        public ActionResult<Almacen> PutAlmacen(string id, [FromBody] Almacen almacenActualizado)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var resultado = _almacenService.Actualizar(id, almacenActualizado);

            if (resultado == null)
            {
                return NotFound(new { mensaje = "El registro no existe o ha sido eliminado." });
            }

            return Ok(resultado);
        }
    }
}