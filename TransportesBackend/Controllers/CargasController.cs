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
    public class CargasController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        public CargasController(TransportesDbContext context)
        {
            _context = context;
        }

        // =================== //
        // GET: api/Cargas     //
        // =================== //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Carga>>> GetCargas()
        {
            var cargas = await _context.Carga
               .Include(c => c.Camion)
               .Include(c => c.Conductor)
               .Include(c => c.CargaPedidos)
                   .ThenInclude(cp => cp.Pedido)
               .OrderByDescending(c => c.FechaSalida)
               .ToListAsync();

            return Ok(cargas);
        }

        // ==================== //
        // POST: api/Cargas     //
        // ==================== //
        [HttpPost]
        public async Task<ActionResult<Carga>> CreateCarga([FromBody] Carga cargaInput)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validar restricción lógica "Arco Exclusivo" Destino
            if (string.IsNullOrEmpty(cargaInput.DestinoAlmacenId) && string.IsNullOrEmpty(cargaInput.DestinoClienteId))
            {
                return BadRequest(new { mensaje = "Debe especificar un destino (Almacén o Cliente)." });
            }
            if (!string.IsNullOrEmpty(cargaInput.DestinoAlmacenId) && !string.IsNullOrEmpty(cargaInput.DestinoClienteId))
            {
                return BadRequest(new { mensaje = "No puede seleccionar Almacén y Cliente a la vez como destino." });
            }

            if (cargaInput.CargaPedidos == null || !cargaInput.CargaPedidos.Any())
            {
                return BadRequest(new { mensaje = "La carga debe tener al menos un pedido asignado." });
            }

            var camion = await _context.Camion.FindAsync(cargaInput.CamionId);
            if (camion == null) return BadRequest(new { mensaje = "El camión no existe." });

            var conductor = await _context.Conductor.FindAsync(cargaInput.ConductorId);
            if (conductor == null) return BadRequest(new { mensaje = "El conductor no existe." });

            var origenAlmacen = await _context.Almacen.FindAsync(cargaInput.OrigenAlmacenId);
            if (origenAlmacen == null) return BadRequest(new { mensaje = "El almacén de origen no existe." });

            // Setear nombres históricos (congelar snapshot de string)
            cargaInput.HistoricoOrigenNombre = origenAlmacen.Nombre;

            if (!string.IsNullOrEmpty(cargaInput.DestinoAlmacenId))
            {
                var destinoAlm = await _context.Almacen.FindAsync(cargaInput.DestinoAlmacenId);
                if (destinoAlm == null) return BadRequest(new { mensaje = "El almacén de destino no existe." });
                cargaInput.HistoricoDestinoNombre = destinoAlm.Nombre;
            }
            else
            {
                var destinoCli = await _context.Cliente.FindAsync(cargaInput.DestinoClienteId);
                if (destinoCli == null) return BadRequest(new { mensaje = "El cliente de destino no existe." });
                cargaInput.HistoricoDestinoNombre = destinoCli.Nombre;
            }

            // Iniciar Transacción para la validación de Capacidad y Creación de Enlaces (Entregas)
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                decimal totalPeso = 0;
                decimal totalVolumen = 0;

                // Validaremos la capacidad sumando todos los productos dentro de los pedidos solicitados
                foreach (var cp in cargaInput.CargaPedidos)
                {
                    var pedidoConDetalles = await _context.Pedido
                        .Include(p => p.PedidoDetalles)
                            .ThenInclude(pd => pd.Producto)
                        .FirstOrDefaultAsync(p => p.Id == cp.PedidoId);

                    if (pedidoConDetalles == null)
                    {
                        throw new Exception($"El pedido {cp.PedidoId} no existe.");
                    }

                    foreach (var detalle in pedidoConDetalles.PedidoDetalles)
                    {
                        totalPeso += detalle.Producto.PesoUnitario * detalle.Cantidad;
                        totalVolumen += detalle.Producto.VolumenUnitario * detalle.Cantidad;
                    }
                }

                if (totalPeso > camion.CapacidadPeso)
                {
                    throw new Exception($"La carga excede el peso maximo del camión. Max: {camion.CapacidadPeso}kg, Calculado: {totalPeso}kg.");
                }

                if (totalVolumen > camion.CapacidadVolumen)
                {
                    throw new Exception($"La carga excede el volumen maximo del camión. Max: {camion.CapacidadVolumen}m3, Calculado: {totalVolumen}m3.");
                }

                // Si pasamos las validaciones, procedemos a crear la carga
                cargaInput.Id = Guid.NewGuid().ToString();

                var nuevaCarga = new Carga
                {
                    Id = cargaInput.Id,
                    CamionId = cargaInput.CamionId,
                    ConductorId = cargaInput.ConductorId,
                    OrigenAlmacenId = cargaInput.OrigenAlmacenId,
                    DestinoAlmacenId = cargaInput.DestinoAlmacenId,
                    DestinoClienteId = cargaInput.DestinoClienteId,
                    HistoricoOrigenNombre = cargaInput.HistoricoOrigenNombre,
                    HistoricoDestinoNombre = cargaInput.HistoricoDestinoNombre,
                    FechaSalida = cargaInput.FechaSalida == default ? DateTime.UtcNow : cargaInput.FechaSalida,
                    FechaLlegadaEstimada = cargaInput.FechaLlegadaEstimada
                };

                _context.Carga.Add(nuevaCarga);
                await _context.SaveChangesAsync();

                // Añadir relativas CargaPedidos y registrar Entregas pendientes para el modulo de Entregas
                foreach (var cp in cargaInput.CargaPedidos)
                {
                    _context.CargaPedido.Add(new CargaPedido
                    {
                        CargaId = nuevaCarga.Id,
                        PedidoId = cp.PedidoId
                    });

                    _context.Entrega.Add(new Entrega
                    {
                        Id = Guid.NewGuid().ToString(),
                        CargaId = nuevaCarga.Id,
                        PedidoId = cp.PedidoId,
                        Estado = "En Camino"
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var cargaCreada = await _context.Carga
                    .Include(c => c.CargaPedidos)
                    .FirstAsync(c => c.Id == nuevaCarga.Id);

                return CreatedAtAction(nameof(GetCargas), new { id = cargaCreada.Id }, cargaCreada);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // ========================== //
        // DELETE: api/Cargas/{id}    //
        // ========================== //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCarga(string id)
        {
            var carga = await _context.Carga.FindAsync(id);
            if (carga == null) return NotFound();

            // SOFT DELETE: Marcamos la baja lógica para mantener historial.
            carga.DeletedAt = DateTime.UtcNow;
            _context.Carga.Update(carga);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
