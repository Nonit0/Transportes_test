using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TransportesBackend.Models;
using TransportesBackend.Services;

namespace TransportesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        // ======================= //
        // GET: api/Productos      //
        // ======================= //
        [HttpGet]
        public ActionResult<IEnumerable<Producto>> GetProductos()
        {
            var productos = _productoService.ObtenerTodos();
            return Ok(productos);
        }

        // ======================= //
        // POST: api/Productos     //
        // ======================= //
        [HttpPost]
        public ActionResult<Producto> PostProducto([FromBody] Producto producto)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (_productoService.ExisteNombre(producto.Nombre))
            {
                return BadRequest(new { mensaje = "Ya existe un producto con este nombre." });
            }

            var nuevoProducto = _productoService.Crear(producto);
            return Ok(nuevoProducto);
        }

        // ======================== //
        // PUT: api/Productos/{id}  //
        // ======================== //
        [HttpPut("{id}")]
        public ActionResult<Producto> PutProducto(string id, [FromBody] Producto productoActualizado)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (_productoService.ExisteNombre(productoActualizado.Nombre, id))
            {
                return BadRequest(new { mensaje = "Otro producto ya usa este nombre." });
            }

            var actualizado = _productoService.Actualizar(id, productoActualizado);
            if (actualizado == null) return NotFound(new { mensaje = "El producto no existe o fue eliminado." });

            return Ok(actualizado);
        }

        // =========================== //
        // DELETE: api/Productos/{id}  //
        // =========================== //
        [HttpDelete("{id}")]
        public IActionResult DeleteProducto(string id)
        {
            var eliminado = _productoService.Eliminar(id);
            if (!eliminado) return NotFound();

            return NoContent();
        }
    }
}
