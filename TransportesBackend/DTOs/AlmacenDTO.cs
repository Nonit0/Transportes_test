using System;

namespace TransportesBackend.DTOs
{
    public class AlmacenDTO
    {
        // Ocultamos explícitamente el 'DeletedAt' y el 'DireccionId' original
        public string Id { get; set; }
        public string Nombre { get; set; }
        
        // Aplanamos la relación con la tabla 'Direccion'
        public string DireccionCompleta { get; set; }
        public string DireccionId { get; set; } 
        /* Ya no hacen falta pues estos pertenecen a direccion no a almacen
        public string Ciudad { get; set; } 
        public string Cp { get; set; } 
        public string Provincia { get; set; } 
        public string Pais { get; set; } 
        */
    }
}