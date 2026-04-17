using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TransportesBackend.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TransportesBackend.Services
{
    public interface IProductoService
    {
        PaginatedResponse<Producto> ObtenerTodos(int? page = null, int? limit = null);
        Producto Crear(Producto producto);
        Producto Actualizar(string id, Producto productoActualizado);
        bool Eliminar(string id);
        bool ExisteNombre(string nombre, string idExcluir = null);
    }

    public class ProductoService : IProductoService
    {
        private readonly TransportesDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductoService(TransportesDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public PaginatedResponse<Producto> ObtenerTodos(int? page = null, int? limit = null)
        {
            var query = _context.Producto.AsQueryable();
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

            var data = query.ToList();
            
            return new PaginatedResponse<Producto> { TotalItems = totalItems, Data = data };
        }

        public Producto Crear(Producto producto)
        {
            if (string.IsNullOrEmpty(producto.Id)) producto.Id = Guid.NewGuid().ToString();
            
            var clienteId = GetClienteId();
            if (!string.IsNullOrEmpty(clienteId))
            {
                producto.ClienteId = clienteId;
            }

            _context.Producto.Add(producto);
            _context.SaveChanges();
            return producto;
        }

        public Producto Actualizar(string id, Producto productoActualizado)
        {
            var producto = _context.Producto.Find(id);
            if (producto == null) return null;

            producto.Nombre = productoActualizado.Nombre;
            producto.Descripcion = productoActualizado.Descripcion;
            producto.PesoUnitario = productoActualizado.PesoUnitario;
            producto.VolumenUnitario = productoActualizado.VolumenUnitario;

            _context.SaveChanges();
            return producto;
        }

        public bool Eliminar(string id)
        {
            var producto = _context.Producto.Find(id);
            if (producto == null) return false;

            producto.DeletedAt = DateTime.UtcNow;
            _context.Producto.Update(producto);
            _context.SaveChanges();
            return true;
        }

        public bool ExisteNombre(string nombre, string idExcluir = null)
        {
            if (idExcluir != null)
            {
                return _context.Producto.Any(p => p.Nombre == nombre && p.Id != idExcluir);
            }
            return _context.Producto.Any(p => p.Nombre == nombre);
        }
    }
}
