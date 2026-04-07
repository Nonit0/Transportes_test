using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TransportesBackend.Models;

namespace TransportesBackend.Services
{
    public interface IDireccionService
    {
        List<Direccion> ObtenerTodas();
        Direccion Crear(Direccion direccion);
    }

    public class DireccionService : IDireccionService
    {
        private readonly TransportesDbContext _context;

        public DireccionService(TransportesDbContext context)
        {
            _context = context;
        }

        public List<Direccion> ObtenerTodas()
        {
            return _context.Direccion
                .AsNoTracking()
                .ToList();
        }

        public Direccion Crear(Direccion entidad)
        {
            if (string.IsNullOrEmpty(entidad.Id)) entidad.Id = System.Guid.NewGuid().ToString();
            _context.Direccion.Add(entidad);
            _context.SaveChanges();
            return entidad;
        }
    }
}
