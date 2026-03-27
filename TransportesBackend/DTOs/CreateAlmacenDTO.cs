
using System;
using System.ComponentModel.DataAnnotations; // Para validaciones

namespace TransportesBackend.DTOs
{
    public class CreateAlmacenDTO
    {
        // Añadimos validaciones básicas para que .NET rechaze peticiones malas automáticamente
        [Required(ErrorMessage = "El campo Nombre es obligatorio")]
        public string Nombre { get; set; } // Ej: Nombre
        
        [Required]
        public string DireccionId { get; set; } // Ej: DireccionId
        
        /* Ya no hacen falta pues estos pertenecen a direccion no a almacen
        [Required]
        public string Ciudad { get; set; } // Ej: Ciudad

        [Required]
        public string Cp { get; set; } // Ej: Cp

        [Required]
        public string Provincia { get; set; } // Ej: Provincia

        [Required]
        public string Pais { get; set; } // Ej: Pais

        
        // ... Solo los campos necesarios para CREAR el registro ...
        */
    }
}