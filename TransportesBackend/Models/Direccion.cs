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
            Almacens = new HashSet<Almacen>();
            Clientes = new HashSet<Cliente>();
            Fabricas = new HashSet<Fabrica>();
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
        [Column("deleted_at", TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; }

        [InverseProperty(nameof(Almacen.Direccion))]
        public virtual ICollection<Almacen> Almacens { get; set; }
        [InverseProperty(nameof(Cliente.Direccion))]
        public virtual ICollection<Cliente> Clientes { get; set; }
        [InverseProperty(nameof(Fabrica.Direccion))]
        public virtual ICollection<Fabrica> Fabricas { get; set; }
    }
}
