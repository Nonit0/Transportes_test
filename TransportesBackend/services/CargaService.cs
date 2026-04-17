using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TransportesBackend.Models;

namespace TransportesBackend.Services
{
    public interface ICargaService
    {
        Task<List<Carga>> ObtenerTodas();
        Task<Carga> Crear(Carga cargaInput);
        Task<bool> Eliminar(string id);
    }

    public class CargaService : ICargaService
    {
        private readonly TransportesDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CargaService(TransportesDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetClienteId() => _httpContextAccessor.HttpContext?.User?.FindFirst("ClienteId")?.Value;
        private string GetUserRole() => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

        public async Task<List<Carga>> ObtenerTodas()
        {
            var query = _context.Carga.AsQueryable();
            var clienteId = GetClienteId();

            if (!string.IsNullOrEmpty(clienteId))
            {
                // Un cliente solo ve cargas donde está involucrado como transportista o destino
                query = query.Where(c => c.Camion.ClienteId == clienteId || 
                                         c.Conductor.ClienteId == clienteId || 
                                         c.DestinoClienteId == clienteId);
            }

            return await query
                .Include(c => c.Camion)
                .Include(c => c.Conductor)
                .Include(c => c.CargaPedidos)
                    .ThenInclude(cp => cp.Pedido)
                .OrderByDescending(c => c.FechaSalida)
                .ToListAsync();
        }

        public async Task<Carga> Crear(Carga cargaInput)
        {
            var clienteId = GetClienteId();

            // Validaciones de seguridad para Cliente
            if (!string.IsNullOrEmpty(clienteId))
            {
                // Verificar que el camión y el conductor le pertenecen
                var camionValido = await _context.Camion.AnyAsync(c => c.Id == cargaInput.CamionId && c.ClienteId == clienteId);
                if (!camionValido) throw new Exception("El camión seleccionado no le pertenece.");

                var conductorValido = await _context.Conductor.AnyAsync(c => c.Id == cargaInput.ConductorId && c.ClienteId == clienteId);
                if (!conductorValido) throw new Exception("El conductor seleccionado no le pertenece.");
            }

            // Validar restricción lógica "Arco Exclusivo" Destino
            if (string.IsNullOrEmpty(cargaInput.DestinoAlmacenId) && string.IsNullOrEmpty(cargaInput.DestinoClienteId))
                throw new Exception("Debe especificar un destino (Almacén o Cliente).");
            
            if (!string.IsNullOrEmpty(cargaInput.DestinoAlmacenId) && !string.IsNullOrEmpty(cargaInput.DestinoClienteId))
                throw new Exception("No puede seleccionar Almacén y Cliente a la vez como destino.");

            if (cargaInput.CargaPedidos == null || !cargaInput.CargaPedidos.Any())
                throw new Exception("La carga debe tener al menos un pedido asignado.");

            var camion = await _context.Camion.FindAsync(cargaInput.CamionId);
            var conductor = await _context.Conductor.FindAsync(cargaInput.ConductorId);
            var origenAlmacen = await _context.Almacen.FindAsync(cargaInput.OrigenAlmacenId);

            cargaInput.HistoricoOrigenNombre = origenAlmacen.Nombre;

            if (!string.IsNullOrEmpty(cargaInput.DestinoAlmacenId))
            {
                var destinoAlm = await _context.Almacen.FindAsync(cargaInput.DestinoAlmacenId);
                cargaInput.HistoricoDestinoNombre = destinoAlm.Nombre;
            }
            else
            {
                var destinoCli = await _context.Cliente.FindAsync(cargaInput.DestinoClienteId);
                cargaInput.HistoricoDestinoNombre = destinoCli.Nombre;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalPeso = 0, totalVolumen = 0;
                foreach (var cp in cargaInput.CargaPedidos)
                {
                    var pedido = await _context.Pedido.Include(p => p.PedidoDetalles).ThenInclude(pd => pd.Producto)
                        .FirstOrDefaultAsync(p => p.Id == cp.PedidoId);
                    
                    if (pedido == null) throw new Exception($"El pedido {cp.PedidoId} no existe.");

                    foreach (var detalle in pedido.PedidoDetalles)
                    {
                        totalPeso += detalle.Producto.PesoUnitario * detalle.Cantidad;
                        totalVolumen += detalle.Producto.VolumenUnitario * detalle.Cantidad;
                    }
                }

                if (totalPeso > camion.CapacidadPeso) throw new Exception("Excede peso máximo.");
                if (totalVolumen > camion.CapacidadVolumen) throw new Exception("Excede volumen máximo.");

                cargaInput.Id = Guid.NewGuid().ToString();
                cargaInput.FechaSalida = cargaInput.FechaSalida == default ? DateTime.UtcNow : cargaInput.FechaSalida;

                _context.Carga.Add(cargaInput);
                await _context.SaveChangesAsync();

                foreach (var cp in cargaInput.CargaPedidos)
                {
                    _context.Entrega.Add(new Entrega
                    {
                        Id = Guid.NewGuid().ToString(),
                        CargaId = cargaInput.Id,
                        PedidoId = cp.PedidoId,
                        Estado = "En Camino"
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await _context.Carga
                    .Include(c => c.CargaPedidos)
                    .FirstAsync(c => c.Id == cargaInput.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Eliminar(string id)
        {
            var carga = await _context.Carga.FindAsync(id);
            if (carga == null) return false;

            var clienteId = GetClienteId();
            if (!string.IsNullOrEmpty(clienteId))
            {
                // Solo puede borrar sus propias cargas (donde es transportista)
                var esTransportista = await _context.Camion.AnyAsync(c => c.Id == carga.CamionId && c.ClienteId == clienteId);
                if (!esTransportista) return false;
            }

            carga.DeletedAt = DateTime.UtcNow;
            _context.Carga.Update(carga);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
