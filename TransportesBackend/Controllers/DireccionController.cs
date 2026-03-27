using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using TransportesBackend.Models;

namespace TransportesBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DireccionesController : ControllerBase
    {
        private readonly TransportesDbContext _context;

        public DireccionesController(TransportesDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDirecciones()
        {
            var direcciones = await _context.Direccion
                .AsNoTracking() // Gracias a esto no se guarda en memoria y permite que el programa sea mas rapido
                .Select(d => new // lo que le enviamos al DTO
                {
                    Id = d.Id,
                    TextoMostrar = d.Calle + " - " + d.Ciudad + " (" + d.Cp + ")"
                })
                .ToListAsync();

            return Ok(direcciones);
        }
        
        [HttpPost]
        public async Task<IActionResult> PostDireccion([FromBody] Direccion nuevaDireccion)
        {
            // Nota rápida: Aquí estamos usando el modelo 'Direccion' directo por simplicidad del Modal, 
            // pero lo ideal en un futuro es usar un CreateDireccionDTO.
            
            _context.Direccion.Add(nuevaDireccion);
            await _context.SaveChangesAsync();

            // Devolvemos el objeto exactamente con la forma de 'DireccionComboDTO' 
            // que Angular espera recibir para meter en el select
            return Ok(new {
                Id = nuevaDireccion.Id,
                TextoMostrar = nuevaDireccion.Calle + " - " + nuevaDireccion.Ciudad + " (" + nuevaDireccion.Cp + ")"
            });
        }
    }
    
}