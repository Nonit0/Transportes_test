using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TransportesBackend.Models;

namespace TransportesBackend.Services
{
    public interface IFabricaService
    {
        List<Fabrica> ObtenerTodas();
        Fabrica Crear(Fabrica fabrica);
        Fabrica Actualizar(string id, Fabrica fabricaActualizada);
        bool Eliminar(string id);
        bool ExisteDireccion(string direccionId);
    }

    public class FabricaService : IFabricaService
    {
        private readonly TransportesDbContext _context;

        public FabricaService(TransportesDbContext context)
        {
            _context = context;
        }

        public List<Fabrica> ObtenerTodas()
        {
            return _context.Fabrica
                .Include(f => f.Direccion)
                .ToList();
        }

        public Fabrica Crear(Fabrica fabrica)
        {
            if (string.IsNullOrEmpty(fabrica.Id)) fabrica.Id = Guid.NewGuid().ToString();
            _context.Fabrica.Add(fabrica);
            _context.SaveChanges();
            return _context.Fabrica
                .Include(f => f.Direccion)
                .First(f => f.Id == fabrica.Id);
        }

        public Fabrica Actualizar(string id, Fabrica fabricaActualizada)
        {
            var entidad = _context.Fabrica.Find(id);
            if (entidad == null) return null;

            entidad.Nombre = fabricaActualizada.Nombre;
            entidad.DireccionId = fabricaActualizada.DireccionId;

            _context.SaveChanges();
            return _context.Fabrica
                .Include(f => f.Direccion)
                .First(f => f.Id == entidad.Id);
        }

        public bool Eliminar(string id)
        {
            var fabrica = _context.Fabrica.Find(id);
            if (fabrica == null) return false;

            fabrica.DeletedAt = DateTime.UtcNow;
            _context.Fabrica.Update(fabrica);
            _context.SaveChanges();
            return true;
        }

        public bool ExisteDireccion(string direccionId)
        {
            return _context.Direccion.Any(d => d.Id == direccionId);
        }
    }
}
