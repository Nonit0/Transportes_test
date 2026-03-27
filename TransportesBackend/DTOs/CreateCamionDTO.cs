using System.ComponentModel.DataAnnotations;

namespace TransportesBackend.DTOs
{
    public class CreateCamionDTO
    {
        [Required(ErrorMessage = "La matrícula es obligatoria")]
        [StringLength(20, ErrorMessage = "La matrícula no puede superar los 20 caracteres")]
        public string Matricula { get; set; }

        [Required(ErrorMessage = "La capacidad de peso es obligatoria")]
        [Range(0.1, 100000, ErrorMessage = "El peso debe ser mayor a 0")]
        public decimal CapacidadPeso { get; set; }

        [Required(ErrorMessage = "La capacidad de volumen es obligatoria")]
        [Range(0.1, 100000, ErrorMessage = "El volumen debe ser mayor a 0")]
        public decimal CapacidadVolumen { get; set; }

        // Lo ponemos en true por defecto para que los nuevos camiones nazcan activos
        public bool Activo { get; set; } = true; 
    }
}
