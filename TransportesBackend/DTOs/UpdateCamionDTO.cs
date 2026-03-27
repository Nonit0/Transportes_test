using System.ComponentModel.DataAnnotations;

namespace TransportesBackend.DTOs
{
    public class UpdateCamionDTO
    {
        [Required(ErrorMessage = "La matrícula es obligatoria")]
        [StringLength(20, ErrorMessage = "La matrícula no puede superar los 20 caracteres")]
        [RegularExpression(@"^[0-9]{4}-[A-Z]{3}$", ErrorMessage = "La matrícula debe tener formato 0000-XXX")]
        public string Matricula { get; set; }

        [Required]
        public decimal CapacidadPeso { get; set; }

        [Required]
        public decimal CapacidadVolumen { get; set; }

        public bool Activo { get; set; }
    }
}
