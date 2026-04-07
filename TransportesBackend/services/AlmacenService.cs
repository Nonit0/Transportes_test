using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TransportesBackend.Models;

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

        public AlmacenService(TransportesDbContext context)
        {
            _context = context;
        }

        // =================== //
        // Obtener todos       //
        // =================== //
        public List<Almacen> ObtenerTodos()
        {
            // Include carga la Dirección asociada en el JSON de salida
            return _context.Almacen
                .Include(a => a.Direccion)
                .ToList();
        }

        // =================== //
        // Crear               //
        // =================== //
        public Almacen Crear(Almacen almacen)
        {
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
