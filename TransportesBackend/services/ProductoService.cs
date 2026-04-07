using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TransportesBackend.Models;

namespace TransportesBackend.Services
{
    public interface IProductoService
    {
        List<Producto> ObtenerTodos();
        Producto Crear(Producto producto);
        Producto Actualizar(string id, Producto productoActualizado);
        bool Eliminar(string id);
        bool ExisteNombre(string nombre, string idExcluir = null);
    }

    public class ProductoService : IProductoService
    {
        private readonly TransportesDbContext _context;

        public ProductoService(TransportesDbContext context)
        {
            _context = context;
        }

        public List<Producto> ObtenerTodos()
        {
            return _context.Producto.ToList();
        }

        public Producto Crear(Producto producto)
        {
            if (string.IsNullOrEmpty(producto.Id)) producto.Id = Guid.NewGuid().ToString();
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
