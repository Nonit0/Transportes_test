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
            CargaPedido = new HashSet<CargaPedido>();
            Entrega = new HashSet<Entrega>();
            PedidoDetalle = new HashSet<PedidoDetalle>();
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

        [ForeignKey(nameof(ClienteId))]
        [InverseProperty("Pedido")]
        public virtual Cliente Cliente { get; set; }
        [InverseProperty("Pedido")]
        public virtual ICollection<CargaPedido> CargaPedido { get; set; }
        [InverseProperty("Pedido")]
        public virtual ICollection<Entrega> Entrega { get; set; }
        [InverseProperty("Pedido")]
        public virtual ICollection<PedidoDetalle> PedidoDetalle { get; set; }
    }
}
