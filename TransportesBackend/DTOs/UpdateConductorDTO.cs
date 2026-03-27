using System.ComponentModel.DataAnnotations;

namespace TransportesBackend.DTOs
{
    public class UpdateConductorDTO
    {
        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(@"^[0-9]{8}[A-Z]$", ErrorMessage = "Formato de DNI inválido (Ej: 12345678A)")]
        public string Dni { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        public string Apellidos { get; set; }

        public string Telefono { get; set; }
    }
}
