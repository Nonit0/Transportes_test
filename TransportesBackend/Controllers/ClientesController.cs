using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TransportesBackend.Models;
using TransportesBackend.Services;

namespace TransportesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        // =================== //
        // GET: api/Clientes   //
        // =================== //
        [HttpGet]
        public ActionResult<IEnumerable<Cliente>> GetClientes()
        {
            var clientes = _clienteService.ObtenerTodos();
            return Ok(clientes);
        }

        // ==================== //
        // POST: api/Clientes   //
        // ==================== //
        [HttpPost]
        public ActionResult<Cliente> CreateCliente([FromBody] Cliente cliente)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!_clienteService.ExisteDireccion(cliente.DireccionId))
            {
                return BadRequest(new { mensaje = "La dirección seleccionada no existe." });
            }

            var clienteCreado = _clienteService.Crear(cliente);
            return CreatedAtAction(nameof(GetClientes), new { id = clienteCreado.Id }, clienteCreado);
        }

        // ======================== //
        // PUT: api/Clientes/{id}   //
        // ======================== //
        [HttpPut("{id}")]
        public ActionResult<Cliente> PutCliente(string id, [FromBody] Cliente clienteActualizado)
        {
            // ModelState comprueba las Data Annotations (etiquetas) que pusiste en tu clase
            // .IsValid comprueba si los datos cumplen las reglas de anotación
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var resultado = _clienteService.Actualizar(id, clienteActualizado);
            if (resultado == null) return NotFound(new { mensaje = "El cliente no existe." });

            return Ok(resultado);
        }

        // ========================== //
        // DELETE: api/Clientes/{id}  //
        // ========================== //
        [HttpDelete("{id}")]
        public IActionResult DeleteCliente(string id)
        {
            var eliminado = _clienteService.Eliminar(id);
            if (!eliminado) return NotFound();

            return NoContent();
        }
    }
}
