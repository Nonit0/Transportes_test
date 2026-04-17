using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using TransportesBackend.Models;
using TransportesBackend.Services;

namespace TransportesBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;

        public PedidosController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        // =================== //
        // GET: api/Pedidos    //
        // =================== //
        [HttpGet]
        public ActionResult<PaginatedResponse<Pedido>> GetPedidos([FromQuery] int? page = null, [FromQuery] int? limit = null)
        {
            var pedidos = _pedidoService.ObtenerTodos(page, limit);
            return Ok(pedidos);
        }

        // ==================== //
        // POST: api/Pedidos    //
        // ==================== //
        [HttpPost]
        public ActionResult<Pedido> CreatePedido([FromBody] Pedido pedidoInput)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validar que el pedido tiene detalles
            if (pedidoInput.PedidoDetalles == null || !pedidoInput.PedidoDetalles.Any())
            {
                return BadRequest(new { mensaje = "El pedido debe contener al menos un producto." });
            }

            // Validar que el cliente existe
            if (!_pedidoService.ExisteCliente(pedidoInput.ClienteId))
            {
                return BadRequest(new { mensaje = "El cliente seleccionado no existe." });
            }

            // Validar que todos los productos existen y tienen cantidad > 0
            foreach (var detalle in pedidoInput.PedidoDetalles)
            {
                if (detalle.Cantidad <= 0)
                    return BadRequest(new { mensaje = "La cantidad de los productos debe ser mayor a 0." });

                if (!_pedidoService.ExisteProducto(detalle.ProductoId))
                    return BadRequest(new { mensaje = $"El producto con ID {detalle.ProductoId} no existe." });
            }

            // Si todo está bien, lo creamos
            var pedidoCreado = _pedidoService.Crear(pedidoInput);
            
            return CreatedAtAction(nameof(GetPedidos), new { id = pedidoCreado.Id }, pedidoCreado);
        }

        // ========================== //
        // DELETE: api/Pedidos/{id}   //
        // ========================== //
        [HttpDelete("{id}")]
        public IActionResult DeletePedido(string id)
        {
            var eliminado = _pedidoService.Eliminar(id);

            if (!eliminado)
            {
                return NotFound(new { mensaje = "El pedido no existe o ya fue eliminado." });
            }

            return NoContent();
        }

        // ============================== //
        // PUT: api/Pedidos/{id}/estado   //
        // ============================== //
        [HttpPut("{id}/estado")]
        public IActionResult CambiarEstado(string id, [FromBody] string nuevoEstado)
        {
            var actualizado = _pedidoService.CambiarEstado(id, nuevoEstado);

            if (!actualizado)
            {
                return NotFound(new { mensaje = "El pedido no existe." });
            }

            return NoContent();
        }
    }
}