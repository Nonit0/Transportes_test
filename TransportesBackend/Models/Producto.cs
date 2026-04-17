using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("producto")]
    [Index(nameof(DeletedAt), nameof(Nombre), Name = "idx_producto_activo")]
    [Index(nameof(Nombre), Name = "nombre", IsUnique = true)]
    public partial class Producto
    {
        public Producto()
        {
            PedidoDetalles = new HashSet<PedidoDetalle>();
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
        [Column("cliente_id")]
        [StringLength(36)]
        public string ClienteId { get; set; }
        [Column("deleted_at", TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(ClienteId))]
        [InverseProperty("Productos")]
        public virtual Cliente Cliente { get; set; }
        [InverseProperty(nameof(PedidoDetalle.Producto))]
        public virtual ICollection<PedidoDetalle> PedidoDetalles { get; set; }
    }
}
