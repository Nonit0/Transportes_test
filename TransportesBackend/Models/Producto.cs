using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("producto")]
    [Index(nameof(Nombre), Name = "nombre", IsUnique = true)]
    public partial class Producto
    {
        public Producto()
        {
            PedidoDetalle = new HashSet<PedidoDetalle>();
        }

        [Key]
        [Column("id")]
        [StringLength(36)]
        public string Id { get; set; }
        [Required]
        [Column("nombre")]
        [StringLength(150)]
        public string Nombre { get; set; }
        [Column("descripcion", TypeName = "text")]
        public string Descripcion { get; set; }
        [Column("peso_unitario")]
        public decimal PesoUnitario { get; set; }
        [Column("volumen_unitario")]
        public decimal VolumenUnitario { get; set; }

        [InverseProperty("Producto")]
        public virtual ICollection<PedidoDetalle> PedidoDetalle { get; set; }
    }
}
