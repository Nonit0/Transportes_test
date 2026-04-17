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
    public class DireccionesController : ControllerBase
    {
        private readonly IDireccionService _direccionService;

        public DireccionesController(IDireccionService direccionService)
        {
            _direccionService = direccionService;
        }

        // ======================= //
        // GET: api/Direcciones    //
        // ======================= //
        [HttpGet]
        public ActionResult<PaginatedResponse<Direccion>> GetDirecciones([FromQuery] int? page = null, [FromQuery] int? limit = null)
        {
            var direcciones = _direccionService.ObtenerTodas(page, limit);
            return Ok(direcciones);
        }
        
        // ======================= //
        // POST: api/Direcciones   //
        // ======================= //
        [HttpPost]
        public ActionResult<Direccion> PostDireccion([FromBody] Direccion nuevaDireccion)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var direccionCreada = _direccionService.Crear(nuevaDireccion);
            return CreatedAtAction(nameof(GetDirecciones), new { id = direccionCreada.Id }, direccionCreada);
        }
    }
}
