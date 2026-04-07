using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using TransportesBackend.Models;

namespace TransportesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DireccionesController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        public DireccionesController(TransportesDbContext context)
        {
            _context = context;
        }

        // ======================= //
        // GET: api/Direcciones    //
        // ======================= //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Direccion>>> GetDirecciones()
        {
            // AsNoTracking no guarda el estado en memoria y permite que el programa sea más rapido
            var direcciones = await _context.Direccion
                .AsNoTracking() 
                .ToListAsync();

            return Ok(direcciones);
        }
        
        // ======================= //
        // POST: api/Direcciones   //
        // ======================= //
        [HttpPost]
        public async Task<ActionResult<Direccion>> PostDireccion([FromBody] Direccion nuevaDireccion)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            _context.Direccion.Add(nuevaDireccion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDirecciones), new { id = nuevaDireccion.Id }, nuevaDireccion);
        }
    }
}