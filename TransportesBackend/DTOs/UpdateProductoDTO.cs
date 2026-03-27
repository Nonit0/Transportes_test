using System.ComponentModel.DataAnnotations;

namespace TransportesBackend.DTOs
{
    public class UpdateProductoDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        public string Descripcion { get; set; } // Puede ser nulo/vacío

        [Required]
        [Range(0.01, 10000, ErrorMessage = "El peso debe ser mayor a 0")]
        public decimal PesoUnitario { get; set; }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "El volumen debe ser mayor a 0")]
        public decimal VolumenUnitario { get; set; }
    }
}
