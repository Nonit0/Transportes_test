using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TransportesBackend.Models;
using Microsoft.AspNetCore.Http;

namespace TransportesBackend.Services
{
    public interface IClienteService
    {
        List<Cliente> ObtenerTodos();
        Cliente Crear(Cliente cliente);
        Cliente Actualizar(string id, Cliente clienteActualizado);
        bool Eliminar(string id);
        bool ExisteDireccion(string direccionId);
    }

    public class ClienteService : IClienteService
    {
        private readonly TransportesDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClienteService(TransportesDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetClienteId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("ClienteId")?.Value;
        }

        public List<Cliente> ObtenerTodos()
        {
            var query = _context.Cliente.AsQueryable();
            var clienteId = GetClienteId();

            if (!string.IsNullOrEmpty(clienteId))
            {
                query = query.Where(c => c.Id == clienteId);
            }

            return query
                .Include(c => c.Direccion)
                .ToList();
        }

        public Cliente Crear(Cliente cliente)
        {
            if (string.IsNullOrEmpty(cliente.Id)) cliente.Id = Guid.NewGuid().ToString();
            _context.Cliente.Add(cliente);
            _context.SaveChanges();
            return _context.Cliente
                .Include(c => c.Direccion)
                .First(c => c.Id == cliente.Id);
        }

        public Cliente Actualizar(string id, Cliente clienteActualizado)
        {
            var cliente = _context.Cliente.Find(id);
            if (cliente == null) return null;

            cliente.Nombre = clienteActualizado.Nombre;
            cliente.DireccionId = clienteActualizado.DireccionId;
            cliente.Telefono = clienteActualizado.Telefono;
            cliente.Email = clienteActualizado.Email;

            _context.SaveChanges();
            return _context.Cliente
                .Include(c => c.Direccion)
                .First(c => c.Id == cliente.Id);
        }

        public bool Eliminar(string id)
        {
            var cliente = _context.Cliente.Find(id);
            if (cliente == null) return false;

            cliente.DeletedAt = DateTime.UtcNow;
            _context.Cliente.Update(cliente);
            _context.SaveChanges();
            return true;
        }

        public bool ExisteDireccion(string direccionId)
        {
            return _context.Direccion.Any(d => d.Id == direccionId);
        }
    }
}
