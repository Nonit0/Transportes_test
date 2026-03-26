using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("direccion")]
    public partial class Direccion
    {
        public Direccion()
        {
            Almacen = new HashSet<Almacen>();
            Cliente = new HashSet<Cliente>();
            Fabrica = new HashSet<Fabrica>();
        }

        [Key]
        [Column("id")]
        [StringLength(36)]
        public string Id { get; set; }
        [Required]
        [Column("calle")]
        [StringLength(200)]
        public string Calle { get; set; }
        [Required]
        [Column("ciudad")]
        [StringLength(100)]
        public string Ciudad { get; set; }
        [Required]
        [Column("provincia")]
        [StringLength(100)]
        public string Provincia { get; set; }
        [Required]
        [Column("cp")]
        [StringLength(10)]
        public string Cp { get; set; }
        [Required]
        [Column("pais")]
        [StringLength(100)]
        public string Pais { get; set; }

        [InverseProperty("Direccion")]
        public virtual ICollection<Almacen> Almacen { get; set; }
        [InverseProperty("Direccion")]
        public virtual ICollection<Cliente> Cliente { get; set; }
        [InverseProperty("Direccion")]
        public virtual ICollection<Fabrica> Fabrica { get; set; }
    }
}
