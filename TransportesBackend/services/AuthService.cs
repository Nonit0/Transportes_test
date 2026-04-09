using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity; // 👈 El nuevo namespace para el cifrado
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using TransportesBackend.Models;

namespace TransportesBackend.Services
{
    public interface IAuthService
    {
        string Login(string email, string password);
        Usuario Registrar(Usuario nuevoUsuario);
    }

    public class AuthService : IAuthService
    {
        private readonly TransportesDbContext _context;
        private readonly IConfiguration _config;
        // Creamos el hasher nativo de Microsoft
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public AuthService(TransportesDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        public string Login(string email, string password)
        {
            // 1. Buscamos al usuario (Síncrono)
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email && u.DeletedAt == null);

            if (usuario == null) return null;

            // 2. VERIFICACIÓN NATIVA DE MICROSOFT
            // Comparamos la contraseña en texto plano con el hash de la BBDD
            var resultado = _passwordHasher.VerifyHashedPassword(usuario, usuario.Password, password);

            // Si el resultado no es Success, la contraseña es mala
            if (resultado == PasswordVerificationResult.Failed)
            {
                return null;
            }

            // 3. Generar Token JWT (Igual que antes)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
                new Claim("ClienteId", usuario.ClienteId ?? "")
            };

            // Debemos declarar en appseetting nuestra clave_super_secreta, de quien recibe y a quien la manda (back y front)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            // seguridad nativa de microsoft
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"], // de donde viene
                audience: _config["Jwt:Audience"], // aquien va
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public Usuario Registrar(Usuario nuevoUsuario)
        {
            // 1. Validar si el email ya existe
            var existe = _context.Usuario.Any(u => u.Email == nuevoUsuario.Email);
            if (existe) return null; // O podrías lanzar una excepción

            // 2. Generar un ID nuevo si no viene uno (UUID)
            if (string.IsNullOrEmpty(nuevoUsuario.Id))
            {
                nuevoUsuario.Id = Guid.NewGuid().ToString();
            }

            // 3. 🛡️ CIFRADO NATIVO: Hasheamos la contraseña
            // El método HashPassword necesita al usuario (puede ser null o el objeto) y la clave plana
            nuevoUsuario.Password = _passwordHasher.HashPassword(nuevoUsuario, nuevoUsuario.Password);

            // 4. Guardar en la base de datos (Síncrono)
            _context.Usuario.Add(nuevoUsuario);
            _context.SaveChanges();

            return nuevoUsuario;
        }
    }
}