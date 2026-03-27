namespace TransportesBackend.DTOs
{
    public class CamionDTO
    {
        public string Id { get; set; }
        public string Matricula { get; set; }
        public decimal CapacidadPeso { get; set; }
        public decimal CapacidadVolumen { get; set; }
        public bool Activo { get; set; }
    }
}
