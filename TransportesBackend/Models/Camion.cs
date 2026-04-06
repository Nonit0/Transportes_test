using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("camion")]
    [Index(nameof(DeletedAt), nameof(Matricula), Name = "idx_camion_activo")]
    [Index(nameof(Matricula), Name = "matricula", IsUnique = true)]
    public partial class Camion
    {
        public Camion()
        {
            Cargas = new HashSet<Carga>();
        }

        [Key]
        [Column("id")]
        [StringLength(36)]
        public string Id { get; set; }
        [Required]
        [Column("matricula")]
        [StringLength(20)]
        public string Matricula { get; set; }
        [Column("capacidad_peso")]
        public decimal CapacidadPeso { get; set; }
        [Column("capacidad_volumen")]
        public decimal CapacidadVolumen { get; set; }
        [Required]
        [Column("activo")]
        public bool? Activo { get; set; }
        [Column("deleted_at", TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; }

        [InverseProperty(nameof(Carga.Camion))]
        public virtual ICollection<Carga> Cargas { get; set; }
    }
}
