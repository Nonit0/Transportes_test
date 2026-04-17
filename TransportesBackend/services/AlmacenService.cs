using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TransportesBackend.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TransportesBackend.Services
{
    // Interfaz: Define el contrato de lo que el servicio sabe hacer
    // El controlador solo conoce esta interfaz, nunca la implementación directa
    public interface IAlmacenService
    {
        List<Almacen> ObtenerTodos();
        Almacen Crear(Almacen almacen);
        Almacen Actualizar(string id, Almacen almacenActualizado);
        bool Eliminar(string id);
        bool ExisteDireccion(string direccionId);
    }

    // Implementación: Aquí va toda la lógica de negocio y acceso a datos
    public class AlmacenService : IAlmacenService
    {
        private readonly TransportesDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AlmacenService(TransportesDbContext context, IHttpContextAccessor httpContextAccessor)
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
        public List<Almacen> ObtenerTodos()
        {
            var query = _context.Almacen.AsQueryable();
            var clienteId = GetClienteId();

            if (!string.IsNullOrEmpty(clienteId))
            {
                query = query.Where(a => a.ClienteId == clienteId);
            }

            return query
                .Include(a => a.Direccion)
                .ToList();
        }

        // =================== //
        // Crear               //
        // =================== //
        public Almacen Crear(Almacen almacen)
        {
            if (string.IsNullOrEmpty(almacen.Id)) almacen.Id = Guid.NewGuid().ToString();
            
            var clienteId = GetClienteId();
            if (!string.IsNullOrEmpty(clienteId))
            {
                almacen.ClienteId = clienteId;
            }

            _context.Almacen.Add(almacen);
            _context.SaveChanges();

            // Recargamos con el Include para devolver la Dirección completa
            return _context.Almacen
                .Include(a => a.Direccion)
                .First(a => a.Id == almacen.Id);
        }

        // =================== //
        // Actualizar          //
        // =================== //
        public Almacen Actualizar(string id, Almacen almacenActualizado)
        {
            var entidad = _context.Almacen.Find(id);

            if (entidad == null) return null;

            // Actualizar datos
            entidad.Nombre = almacenActualizado.Nombre;
            entidad.DireccionId = almacenActualizado.DireccionId;

            _context.SaveChanges();

            // Recargamos con Include para devolver la Dirección completa
            return _context.Almacen
                .Include(a => a.Direccion)
                .First(a => a.Id == entidad.Id);
        }

        // =================== //
        // Eliminar (Soft)     //
        // =================== //
        public bool Eliminar(string id)
        {
            var almacen = _context.Almacen.Find(id);

            if (almacen == null) return false;

            // SOFT DELETE: Marcamos la baja lógica para mantener historial.
            almacen.DeletedAt = DateTime.UtcNow;
            _context.Almacen.Update(almacen);
            _context.SaveChanges();

            return true;
        }

        // =================== //
        // Validación auxiliar  //
        // =================== //
        public bool ExisteDireccion(string direccionId)
        {
            return _context.Direccion.Any(d => d.Id == direccionId);
        }
    }
}
