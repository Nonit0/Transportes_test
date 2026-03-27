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
    public class ProductosController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        public ProductosController(TransportesDbContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDTO>>> GetProductos()
        {
            var productos = await _context.Producto
                .Select(p => new ProductoDTO {
                    Id = p.Id, Nombre = p.Nombre, Descripcion = p.Descripcion,
                    PesoUnitario = p.PesoUnitario, VolumenUnitario = p.VolumenUnitario
                }).ToListAsync();
            return Ok(productos);
        }

        [HttpPost]
        public async Task<ActionResult<ProductoDTO>> PostProducto([FromBody] CreateProductoDTO dto)
        {
            if (await _context.Producto.AnyAsync(p => p.Nombre == dto.Nombre))
                return BadRequest(new { mensaje = "Ya existe un producto con este nombre." });

            var producto = new Producto {
                Nombre = dto.Nombre, Descripcion = dto.Descripcion,
                PesoUnitario = dto.PesoUnitario, VolumenUnitario = dto.VolumenUnitario
            };

            _context.Producto.Add(producto);
            await _context.SaveChangesAsync();

            return Ok(new ProductoDTO { Id = producto.Id, Nombre = producto.Nombre, Descripcion = producto.Descripcion, PesoUnitario = producto.PesoUnitario, VolumenUnitario = producto.VolumenUnitario });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(string id, [FromBody] UpdateProductoDTO dto)
        {
            var producto = await _context.Producto.FindAsync(id);
            if (producto == null) return NotFound();

            if (await _context.Producto.AnyAsync(p => p.Nombre == dto.Nombre && p.Id != id))
                return BadRequest(new { mensaje = "Otro producto ya usa este nombre." });

            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.PesoUnitario = dto.PesoUnitario;
            producto.VolumenUnitario = dto.VolumenUnitario;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(string id)
        {
            var producto = await _context.Producto.FindAsync(id);
            if (producto == null) return NotFound();

            _context.Producto.Remove(producto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
