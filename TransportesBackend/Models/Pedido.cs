using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("pedido")]
    [Index(nameof(ClienteId), Name = "idx_pedido_cliente")]
    public partial class Pedido
    {
        public Pedido()
        {
            CargaPedidos = new HashSet<CargaPedido>();
            Entregas = new HashSet<Entrega>();
            PedidoDetalles = new HashSet<PedidoDetalle>();
        }

        [Key]
        [Column("id")]
        [StringLength(36)]
        public string Id { get; set; }
        [Required]
        [Column("cliente_id")]
        [StringLength(36)]
        public string ClienteId { get; set; }
        [Column("fecha_pedido", TypeName = "date")]
        public DateTime FechaPedido { get; set; }
        [Required]
        [Column("estado")]
        [StringLength(30)]
        public string Estado { get; set; }
        [Column("deleted_at", TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(ClienteId))]
        [InverseProperty("Pedidos")]
        public virtual Cliente Cliente { get; set; }
        [InverseProperty(nameof(CargaPedido.Pedido))]
        public virtual ICollection<CargaPedido> CargaPedidos { get; set; }
        [InverseProperty(nameof(Entrega.Pedido))]
        public virtual ICollection<Entrega> Entregas { get; set; }
        [InverseProperty(nameof(PedidoDetalle.Pedido))]
        public virtual ICollection<PedidoDetalle> PedidoDetalles { get; set; }
    }
}
