using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("fabrica")]
    [Index(nameof(DireccionId), Name = "fk_fab_dir")]
    [Index(nameof(Nombre), Name = "nombre", IsUnique = true)]
    public partial class Fabrica
    {
        [Key]
        [Column("id")]
        [StringLength(36)]
        public string Id { get; set; }
        [Required]
        [Column("nombre")]
        [StringLength(150)]
        public string Nombre { get; set; }
        [Required]
        [Column("direccion_id")]
        [StringLength(36)]
        public string DireccionId { get; set; }

        [ForeignKey(nameof(DireccionId))]
        [InverseProperty("Fabrica")]
        public virtual Direccion Direccion { get; set; }
    }
}
