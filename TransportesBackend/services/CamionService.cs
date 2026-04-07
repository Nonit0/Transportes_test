using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TransportesBackend.Models;

namespace TransportesBackend.Services
{
    public interface ICamionService
    {
        List<Camion> ObtenerTodos();
        Camion Crear(Camion camion);
        Camion Actualizar(string id, Camion camionActualizado);
        bool Eliminar(string id);
        bool ExisteMatricula(string matricula, string idExcluir = null);
    }

    public class CamionService : ICamionService
    {
        private readonly TransportesDbContext _context;

        public CamionService(TransportesDbContext context)
        {
            _context = context;
        }

        public List<Camion> ObtenerTodos()
        {
            return _context.Camion
                .IgnoreQueryFilters()
                .OrderByDescending(c => c.Activo)
                .ToList();
        }

        public Camion Crear(Camion camion)
        {
            if (string.IsNullOrEmpty(camion.Id)) camion.Id = Guid.NewGuid().ToString();
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
