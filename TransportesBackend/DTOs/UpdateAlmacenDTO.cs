using System.ComponentModel.DataAnnotations;

namespace TransportesBackend.DTOs
{
    public class UpdateAlmacenDTO
    {
        [Required(ErrorMessage = "El campo es obligatorio")]
        public string Nombre { get; set; } 
        
        [Required]
        public string DireccionId { get; set; } 

        /* Ya no hacen falta pues estos pertenecen a direccion no a almacen
        [Required]
        public string Ciudad { get; set; } 

        [Required]
        public string Cp { get; set; } 

        [Required]
        public string Provincia { get; set; } 

        [Required]
        public string Pais { get; set; } 
        */
    }
}