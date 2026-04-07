using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TransportesBackend.Models;

namespace TransportesBackend.Services
{
    public interface IConductorService
    {
        List<Conductor> ObtenerTodos();
        Conductor Crear(Conductor conductor);
        Conductor Actualizar(string id, Conductor conductorActualizado);
        bool Eliminar(string id);
        bool ExisteDni(string dni, string idExcluir = null);
    }

    public class ConductorService : IConductorService
    {
        private readonly TransportesDbContext _context;

        public ConductorService(TransportesDbContext context)
        {
            _context = context;
        }

        public List<Conductor> ObtenerTodos()
        {
            return _context.Conductor.ToList();
        }

        public Conductor Crear(Conductor conductor)
        {
            if (string.IsNullOrEmpty(conductor.Id)) conductor.Id = Guid.NewGuid().ToString();
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
