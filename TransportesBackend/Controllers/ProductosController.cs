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
    public class ProductosController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        public ProductosController(TransportesDbContext context) { _context = context; }

        // ======================= //
        // GET: api/Productos      //
        // ======================= //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            var productos = await _context.Producto.ToListAsync();
            return Ok(productos);
        }

        // ======================= //
        // POST: api/Productos     //
        // ======================= //
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto([FromBody] Producto producto)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Producto.AnyAsync(p => p.Nombre == producto.Nombre))
                return BadRequest(new { mensaje = "Ya existe un producto con este nombre." });

            _context.Producto.Add(producto);
            await _context.SaveChangesAsync();

            return Ok(producto);
        }

        // ======================== //
        // PUT: api/Productos/{id}  //
        // ======================== //
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(string id, [FromBody] Producto productoActualizado)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var producto = await _context.Producto.FindAsync(id);
            if (producto == null) return NotFound();

            if (await _context.Producto.AnyAsync(p => p.Nombre == productoActualizado.Nombre && p.Id != id))
                return BadRequest(new { mensaje = "Otro producto ya usa este nombre." });

            producto.Nombre = productoActualizado.Nombre;
            producto.Descripcion = productoActualizado.Descripcion;
            producto.PesoUnitario = productoActualizado.PesoUnitario;
            producto.VolumenUnitario = productoActualizado.VolumenUnitario;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =========================== //
        // DELETE: api/Productos/{id}  //
        // =========================== //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(string id)
        {
            var producto = await _context.Producto.FindAsync(id);
            if (producto == null) return NotFound();

            // SOFT DELETE: Marcamos la baja lógica para mantener historial.
            producto.DeletedAt = System.DateTime.UtcNow;
            _context.Producto.Update(producto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
