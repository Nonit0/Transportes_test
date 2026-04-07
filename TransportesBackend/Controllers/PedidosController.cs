using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TransportesBackend.Models;
using System.Linq;
using System.Collections.Generic;
using System;

namespace TransportesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        public PedidosController(TransportesDbContext context)
        {
            _context = context;
        }

        // =================== //
        // GET: api/Pedidos    //
        // =================== //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            var pedidos = await _context.Pedido
               .Include(p => p.Cliente)
               .Include(p => p.PedidoDetalles)
                   .ThenInclude(pd => pd.Producto)
               .OrderByDescending(p => p.FechaPedido)
               .ToListAsync();

            return Ok(pedidos);
        }

        // ==================== //
        // POST: api/Pedidos    //
        // ==================== //
        [HttpPost]
        public async Task<ActionResult<Pedido>> CreatePedido([FromBody] Pedido pedidoInput)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (pedidoInput.PedidoDetalles == null || !pedidoInput.PedidoDetalles.Any())
            {
                return BadRequest(new { mensaje = "El pedido debe contener al menos un producto." });
            }

            var clienteExiste = await _context.Cliente.AnyAsync(c => c.Id == pedidoInput.ClienteId);
            if (!clienteExiste) return BadRequest(new { mensaje = "El cliente seleccionado no existe." });

            // Iniciar Transacción Explícita (Requisito)
            // Crea atomicidad a la hora de hacer el pedido, o lo guarda con todo o no guarda nada
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Crear el Pedido Maestro base
                var nuevoPedido = new Pedido
                {
                    Id = Guid.NewGuid().ToString(),
                    ClienteId = pedidoInput.ClienteId,
                    FechaPedido = pedidoInput.FechaPedido == default ? DateTime.UtcNow : pedidoInput.FechaPedido,
                    Estado = "Pendiente" // Valor por defecto
                };

                // Guardamos el pedido base
                _context.Pedido.Add(nuevoPedido);
                await _context.SaveChangesAsync();

                // 2. Insertar Detalles
                foreach (var detalleInput in pedidoInput.PedidoDetalles)
                {
                    // Comprobaciones de cantidad
                    if (detalleInput.Cantidad <= 0)
                    {
                        throw new Exception("La cantidad de los productos debe ser mayor a 0.");
                    }

                    // Comprobar que el producto existe
                    var productoExiste = await _context.Producto.AnyAsync(p => p.Id == detalleInput.ProductoId);
                    if (!productoExiste)
                    {
                        throw new Exception($"El producto con ID {detalleInput.ProductoId} no existe.");
                    }

                    // Crear el detalle del pedido
                    var nuevoDetalle = new PedidoDetalle
                    {
                        Id = Guid.NewGuid().ToString(),
                        PedidoId = nuevoPedido.Id,
                        ProductoId = detalleInput.ProductoId,
                        Cantidad = detalleInput.Cantidad
                    };
                    
                    // Guardamos el detalle del pedido
                    _context.PedidoDetalle.Add(nuevoDetalle);
                }

                // Guardamos todos los cambios
                await _context.SaveChangesAsync();
                
                // Si todo fue exitoso, confirmamos la transacción
                // Si hay un error, se ejecuta el catch y se revierte la transacción asegurando el todo o nada
                await transaction.CommitAsync();

                // Devolvemos el pedido con sus includes para que el frontend pueda mostrarlo
                var pedidoCreado = await _context.Pedido
                    .Include(p => p.Cliente)
                    .Include(p => p.PedidoDetalles)
                        .ThenInclude(pd => pd.Producto)
                    .FirstAsync(p => p.Id == nuevoPedido.Id);

                // Devolvemos el pedido creado con codigo 201
                return CreatedAtAction(
                    nameof(GetPedidos),           // 1. ¿A qué método tengo que llamar para ver este pedido?
                    new { id = pedidoCreado.Id }, // 2. ¿Qué parámetro necesita ese método?
                    pedidoCreado                  // 3. El objeto completo que le devolvemos a Angular ahora mismo
                );
            }
            catch (Exception ex)
            {
                // Revertimos la creación del pedido si hay fallo
                await transaction.RollbackAsync();
                // Devolvemos el error con codigo 400
                return BadRequest(new { mensaje = "Error al crear el pedido: " + ex.Message });
            }
        }

        // ========================== //
        // DELETE: api/Pedidos/{id}   //
        // ========================== //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedido(string id)
        {
            var pedido = await _context.Pedido.FindAsync(id);
            if (pedido == null) return NotFound(new { mensaje = "El pedido no existe." });

            // SOFT DELETE: Marcamos la baja lógica para mantener historial.
            pedido.DeletedAt = DateTime.UtcNow;
            _context.Pedido.Update(pedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ========================== //
        // PUT: api/Pedidos/{id}/Estado//
        // ========================== //
        // Helper para cambiar estado del pedido rápidamente (ej. cancelarlo)
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(string id, [FromBody] string nuevoEstado)
        {
            var pedido = await _context.Pedido.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Estado = nuevoEstado;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
