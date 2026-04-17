using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TransportesBackend.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TransportesBackend.Services
{
    public interface ICamionService
    {
        PaginatedResponse<Camion> ObtenerTodos(int? page = null, int? limit = null);
        Camion Crear(Camion camion);
        Camion Actualizar(string id, Camion camionActualizado);
        bool Eliminar(string id);
        bool ExisteMatricula(string matricula, string idExcluir = null);
    }

    public class CamionService : ICamionService
    {
        private readonly TransportesDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CamionService(TransportesDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public PaginatedResponse<Camion> ObtenerTodos(int? page = null, int? limit = null)
        {
            var query = _context.Camion.IgnoreQueryFilters().AsQueryable();
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

            var data = query
                .OrderByDescending(c => c.Activo)
                .ToList();
                
            return new PaginatedResponse<Camion> { TotalItems = totalItems, Data = data };
        }

        public Camion Crear(Camion camion)
        {
            if (string.IsNullOrEmpty(camion.Id)) camion.Id = Guid.NewGuid().ToString();
            
            var clienteId = GetClienteId();
            if (!string.IsNullOrEmpty(clienteId))
            {
                camion.ClienteId = clienteId;
            }

            _context.Camion.Add(camion);
            _context.SaveChanges();
            return camion;
        }

        public Camion Actualizar(string id, Camion camionActualizado)
        {
            var entidad = _context.Camion.IgnoreQueryFilters().FirstOrDefault(c => c.Id == id);
            if (entidad == null) return null;

            entidad.Matricula = camionActualizado.Matricula;
            entidad.CapacidadPeso = camionActualizado.CapacidadPeso;
            entidad.CapacidadVolumen = camionActualizado.CapacidadVolumen;
            entidad.Activo = camionActualizado.Activo;
            // No actualizamos DeletedAt aquí, se hace en Eliminar

            _context.SaveChanges();
            return entidad;
        }

        public bool Eliminar(string id)
        {
            System.Console.WriteLine($"DEBUG ELIMINAR: buscando ID '{id}'");
            var camion = _context.Camion.IgnoreQueryFilters().FirstOrDefault(c => c.Id == id);
            if (camion == null) 
            {
                System.Console.WriteLine($"DEBUG ELIMINAR: ID '{id}' NO ENCONTRADO");
                return false;
            }

            camion.Activo = false;
            camion.DeletedAt = DateTime.UtcNow;
            _context.Camion.Update(camion);
            _context.SaveChanges();
            return true;
        }

        public bool ExisteMatricula(string matricula, string idExcluir = null)
        {
            if (idExcluir != null)
            {
                return _context.Camion.Any(c => c.Matricula == matricula && c.Id != idExcluir);
            }
            return _context.Camion.Any(c => c.Matricula == matricula);
        }
    }
}
