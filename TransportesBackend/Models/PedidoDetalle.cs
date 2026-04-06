using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("pedido_detalle")]
    [Index(nameof(PedidoId), Name = "fk_pd_pedido")]
    [Index(nameof(ProductoId), Name = "fk_pd_producto")]
    public partial class PedidoDetalle
    {
        [Key]
        [Column("id")]
        [StringLength(36)]
        public string Id { get; set; }
        [Required]
        [Column("pedido_id")]
        [StringLength(36)]
        public string PedidoId { get; set; }
        [Required]
        [Column("producto_id")]
        [StringLength(36)]
        public string ProductoId { get; set; }
        [Column("cantidad", TypeName = "int(11)")]
        public int Cantidad { get; set; }

        [ForeignKey(nameof(PedidoId))]
        [InverseProperty("PedidoDetalles")]
        public virtual Pedido Pedido { get; set; }
        [ForeignKey(nameof(ProductoId))]
        [InverseProperty("PedidoDetalles")]
        public virtual Producto Producto { get; set; }
    }
}
