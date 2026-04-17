using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TransportesBackend.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TransportesBackend.Services
{
    public interface IPedidoService
    {
        PaginatedResponse<Pedido> ObtenerTodos(int? page = null, int? limit = null);
        Pedido Crear(Pedido pedido);
        bool Eliminar(string id);
        bool CambiarEstado(string id, string nuevoEstado);
        bool EntregarPedido(string id);
        bool MarcarEnEnvio(string pedidoId);
        bool ExisteCliente(string clienteId);
        bool ExisteProducto(string productoId);
    }

    public class PedidoService : IPedidoService
    {
        private readonly TransportesDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PedidoService(TransportesDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetClienteId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("ClienteId")?.Value;
        }

        private string GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
        }

        // =================== //
        // Obtener todos       //
        // =================== //
        public PaginatedResponse<Pedido> ObtenerTodos(int? page = null, int? limit = null)
        {
            var query = _context.Pedido.IgnoreQueryFilters().AsQueryable();
            var clienteId = GetClienteId();

            if (!string.IsNullOrEmpty(clienteId))
            {
                query = query.Where(p => p.ClienteId == clienteId);
            }

            int totalItems = query.Count();

            if (page.HasValue && limit.HasValue && page.Value > 0 && limit.Value > 0)
            {
                query = query.Skip((page.Value - 1) * limit.Value).Take(limit.Value);
            }

            var data = query
                .Include(p => p.Cliente)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(pd => pd.Producto)
                .OrderByDescending(p => p.FechaPedido)
                .ToList();
                
            return new PaginatedResponse<Pedido> { TotalItems = totalItems, Data = data };
        }

        // =================== //
        // Crear               //
        // =================== //
        public Pedido Crear(Pedido pedidoInput)
        {
            // 1. Generar ID y datos base
            pedidoInput.Id = Guid.NewGuid().ToString();
            pedidoInput.FechaPedido = pedidoInput.FechaPedido == default ? DateTime.UtcNow : pedidoInput.FechaPedido;
            pedidoInput.Estado = "Pendiente";

            // 2. Preparar los detalles
            foreach (var detalle in pedidoInput.PedidoDetalles)
            {
                detalle.Id = Guid.NewGuid().ToString();
                detalle.PedidoId = pedidoInput.Id;
            }

            // Si es un cliente, forzamos que el ClienteId sea el suyo
            var clienteId = GetClienteId();
            if (!string.IsNullOrEmpty(clienteId))
            {
                pedidoInput.ClienteId = clienteId;
            }

            // 3. Añadir el pedido (EF Core añade los detalles automáticamente porque están dentro del pedido)
            _context.Pedido.Add(pedidoInput);
            
            // 4. Guardar todo de golpe en la base de datos (Todo o nada)
            _context.SaveChanges();

            // 5. Devolver el objeto completo con sus relaciones
            return _context.Pedido
                .Include(p => p.Cliente)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(pd => pd.Producto)
                .First(p => p.Id == pedidoInput.Id);
        }

        // =================== //
        // Eliminar (Soft)     //
        // =================== //
        public bool Eliminar(string id)
        {
            var pedido = _context.Pedido.Find(id); // Síncrono

            if (pedido == null) return false;

            // Baja lógica
            pedido.DeletedAt = DateTime.UtcNow;
            _context.Pedido.Update(pedido);
            _context.SaveChanges();

            return true;
        }

        // =================== //
        // Cambiar Estado      //
        // =================== //
        public bool CambiarEstado(string id, string nuevoEstado)
        {
            var pedido = _context.Pedido.Find(id);
            if (pedido == null) return false;
            pedido.Estado = nuevoEstado;
            _context.SaveChanges();
            return true;
        }

        // ========================= //
        // Entregar (Pedido Final)   //
        // ========================= //
        public bool EntregarPedido(string id)
        {
            var pedido = _context.Pedido.Find(id);
            if (pedido == null) return false;

            // Solo se puede entregar si está "En Envío"
            if (pedido.Estado != "En Envío")
                throw new InvalidOperationException("El pedido debe estar en estado 'En Envío' para poder entregarlo.");

            // Buscar el registro de Entrega activo asociado
            var entrega = _context.Entrega.FirstOrDefault(e => e.PedidoId == id && e.FechaEntrega == null);
            if (entrega != null)
            {
                entrega.FechaEntrega = DateTime.UtcNow;
                entrega.Estado = "Entregado";
                _context.Entrega.Update(entrega);
            }

            pedido.Estado = "Entregado";
            _context.Pedido.Update(pedido);
            _context.SaveChanges();
            return true;
        }

        // ========================= //
        // Marcar En Envío          //
        // ========================= //
        public bool MarcarEnEnvio(string pedidoId)
        {
            var pedido = _context.Pedido.Find(pedidoId);
            if (pedido == null) return false;
            pedido.Estado = "En Envío";
            _context.Pedido.Update(pedido);
            _context.SaveChanges();
            return true;
        }

        // =================== //
        // Validaciones        //
        // =================== //
        public bool ExisteCliente(string clienteId)
        {
            return _context.Cliente.Any(c => c.Id == clienteId);
        }

        public bool ExisteProducto(string productoId)
        {
            return _context.Producto.Any(p => p.Id == productoId);
        }
    }
}