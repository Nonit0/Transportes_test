using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TransportesBackend.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TransportesBackend.Services
{
    public interface IConductorService
    {
        PaginatedResponse<Conductor> ObtenerTodos(int? page = null, int? limit = null);
        Conductor Crear(Conductor conductor);
        Conductor Actualizar(string id, Conductor conductorActualizado);
        bool Eliminar(string id);
        bool ExisteDni(string dni, string idExcluir = null);
    }

    public class ConductorService : IConductorService
    {
        private readonly TransportesDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConductorService(TransportesDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public PaginatedResponse<Conductor> ObtenerTodos(int? page = null, int? limit = null)
        {
            var query = _context.Conductor.AsQueryable();
            var clienteId = GetClienteId();

            if (!string.IsNullOrEmpty(clienteId))
            {
                query = query.Where(c => c.ClienteId == clienteId);
            }

            int totalItems = query.Count();

            if (page.HasValue && limit.HasValue && page.Value > 0 && limit.Value > 0)
            {
                query = query.Skip((page.Value - 1) * limit.Value).Take(limit.Value);
            }

            var data = query.ToList();
            
            return new PaginatedResponse<Conductor> { TotalItems = totalItems, Data = data };
        }

        public Conductor Crear(Conductor conductor)
        {
            if (string.IsNullOrEmpty(conductor.Id)) conductor.Id = Guid.NewGuid().ToString();
            
            var clienteId = GetClienteId();
            if (!string.IsNullOrEmpty(clienteId))
            {
                conductor.ClienteId = clienteId;
            }

            _context.Conductor.Add(conductor);
            _context.SaveChanges();
            return conductor;
        }

        public Conductor Actualizar(string id, Conductor conductorActualizado)
        {
            var conductor = _context.Conductor.Find(id);
            if (conductor == null) return null;

            conductor.Dni = conductorActualizado.Dni;
            conductor.Nombre = conductorActualizado.Nombre;
            conductor.Apellidos = conductorActualizado.Apellidos;
            conductor.Telefono = conductorActualizado.Telefono;

            _context.SaveChanges();
            return conductor;
        }

        public bool Eliminar(string id)
        {
            var conductor = _context.Conductor.Find(id);
            if (conductor == null) return false;

            conductor.DeletedAt = DateTime.UtcNow;
            _context.Conductor.Update(conductor);
            _context.SaveChanges();
            return true;
        }

        public bool ExisteDni(string dni, string idExcluir = null)
        {
            if (idExcluir != null)
            {
                return _context.Conductor.Any(c => c.Dni == dni && c.Id != idExcluir);
            }
            return _context.Conductor.Any(c => c.Dni == dni);
        }
    }
}
