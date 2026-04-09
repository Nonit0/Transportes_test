using Microsoft.AspNetCore.Mvc;
using TransportesBackend.Services;
using TransportesBackend.Models; // 👈 Esto le dice dónde está la clase Usuario

namespace TransportesBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Clase auxiliar rápida para recibir los datos
        public class Credenciales 
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Credenciales datos)
        {
            // Llamamos al servicio (Síncrono)
            var token = _authService.Login(datos.Email, datos.Password);

            if (token == null)
            {
                return Unauthorized(new { mensaje = "Email o contraseña incorrectos" });
            }

            // Devolvemos el token al frontend
            return Ok(new { token });
        }

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] Usuario usuario)
        {
            // Llamamos al servicio para que haga la magia del hash y el guardado
            var resultado = _authService.Registrar(usuario);

            if (resultado == null)
            {
                return BadRequest(new { mensaje = "El email ya está registrado en el sistema." });
            }

            // Por seguridad, no devolvemos la contraseña hasheada al frontend
            resultado.Password = null;

            return Ok(new { mensaje = "Usuario registrado con éxito", usuario = resultado });
        }
    }
}