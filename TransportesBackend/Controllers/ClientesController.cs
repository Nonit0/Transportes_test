using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TransportesBackend.Models;
using System.Linq;
using System.Collections.Generic;
using System;

namespace TransportesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        public ClientesController(TransportesDbContext context)
        {
            _context = context;
        }

        // =================== //
        // GET: api/Clientes   //
        // =================== //
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            var clientes = await _context.Cliente
                .Include(c => c.Direccion)
                .ToListAsync();

            return Ok(clientes);
        }

        // ==================== //
        // POST: api/Clientes   //
        // ==================== //
        [HttpPost]
        public async Task<ActionResult<Cliente>> CreateCliente([FromBody] Cliente cliente)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Verificar que la dirección existe
            var direccionExiste = await _context.Direccion.AnyAsync(d => d.Id == cliente.DireccionId);
            if (!direccionExiste)
            {
                return BadRequest(new { mensaje = "La dirección seleccionada no existe." });
            }

            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();

            // Recargamos con Include para devolver la Dirección
            var clienteCreado = await _context.Cliente
                .Include(c => c.Direccion)
                .FirstAsync(c => c.Id == cliente.Id);

            return CreatedAtAction(nameof(GetClientes), new { id = clienteCreado.Id }, clienteCreado);
        }

        // ======================== //
        // PUT: api/Clientes/{id}   //
        // ======================== //
        [HttpPut("{id}")]
        public async Task<ActionResult<Cliente>> PutCliente(string id, [FromBody] Cliente clienteActualizado)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var cliente = await _context.Cliente.FindAsync(id);
            if (cliente == null) return NotFound(new { mensaje = "El cliente no existe." });

            cliente.Nombre = clienteActualizado.Nombre;
            cliente.DireccionId = clienteActualizado.DireccionId;
            cliente.Telefono = clienteActualizado.Telefono;
            cliente.Email = clienteActualizado.Email;

            await _context.SaveChangesAsync();

            var resultado = await _context.Cliente
                .Include(c => c.Direccion)
                .FirstAsync(c => c.Id == cliente.Id);

            return Ok(resultado);
        }

        // ========================== //
        // DELETE: api/Clientes/{id}  //
        // ========================== //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(string id)
        {
            var cliente = await _context.Cliente.FindAsync(id);
            if (cliente == null) return NotFound();

            // SOFT DELETE: Marcamos la baja lógica para mantener historial.
            cliente.DeletedAt = DateTime.UtcNow;
            _context.Cliente.Update(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
