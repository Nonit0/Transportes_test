using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DireccionService(TransportesDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetClienteId() => _httpContextAccessor.HttpContext?.User?.FindFirst("ClienteId")?.Value;
        private string GetUserRole() => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

        public List<Direccion> ObtenerTodas()
        {
            var query = _context.Direccion.AsNoTracking().AsQueryable();
            var clienteId = GetClienteId();

            if (!string.IsNullOrEmpty(clienteId))
            {
                // Obtenemos todos los IDs de direcciones vinculadas a este cliente
                var direccionIdsRelacionadas = _context.Almacen.Where(a => a.ClienteId == clienteId).Select(a => a.DireccionId)
                    .Union(_context.Fabrica.Where(f => f.ClienteId == clienteId).Select(f => f.DireccionId))
                    .Union(_context.Cliente.Where(c => c.Id == clienteId).Select(c => c.DireccionId));

                query = query.Where(d => direccionIdsRelacionadas.Contains(d.Id));
            }

            return query.ToList();
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
