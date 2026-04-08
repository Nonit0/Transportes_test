using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TransportesBackend.Models;

namespace TransportesBackend.Services
{
    public interface IPedidoService
    {
        List<Pedido> ObtenerTodos();
        Pedido Crear(Pedido pedido);
        bool Eliminar(string id);
        bool CambiarEstado(string id, string nuevoEstado);
        bool ExisteCliente(string clienteId);
        bool ExisteProducto(string productoId);
    }

    public class PedidoService : IPedidoService
    {
        private readonly TransportesDbContext _context;

        public PedidoService(TransportesDbContext context)
        {
            _context = context;
        }

        // =================== //
        // Obtener todos       //
        // =================== //
        public List<Pedido> ObtenerTodos()
        {
            return _context.Pedido
            .IgnoreQueryFilters()
                .Include(p => p.Cliente)
                .Include(p => p.PedidoDetalles)
                    .ThenInclude(pd => pd.Producto)
                .OrderByDescending(p => p.FechaPedido)
                .ToList(); // 100% síncrono
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