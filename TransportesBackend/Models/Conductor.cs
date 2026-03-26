using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("conductor")]
    [Index(nameof(Dni), Name = "dni", IsUnique = true)]
    public partial class Conductor
    {
        public Conductor()
        {
            Carga = new HashSet<Carga>();
        }

        [Key]
        [Column("id")]
        [StringLength(36)]
        public string Id { get; set; }
        [Required]
        [Column("dni")]
        [StringLength(15)]
        public string Dni { get; set; }
        [Required]
        [Column("nombre")]
        [StringLength(100)]
        public string Nombre { get; set; }
        [Required]
        [Column("apellidos")]
        [StringLength(150)]
        public string Apellidos { get; set; }
        [Column("telefono")]
        [StringLength(20)]
        public string Telefono { get; set; }

        [InverseProperty("Conductor")]
        public virtual ICollection<Carga> Carga { get; set; }
    }
}
