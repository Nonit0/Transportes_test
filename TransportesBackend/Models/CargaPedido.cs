using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("carga_pedido")]
    [Index(nameof(PedidoId), Name = "fk_cp_pedido")]
    public partial class CargaPedido
    {
        [Key]
        [Column("carga_id")]
        [StringLength(36)]
        public string CargaId { get; set; }
        [Key]
        [Column("pedido_id")]
        [StringLength(36)]
        public string PedidoId { get; set; }

        [ForeignKey(nameof(CargaId))]
        [InverseProperty("CargaPedido")]
        public virtual Carga Carga { get; set; }
        [ForeignKey(nameof(PedidoId))]
        [InverseProperty("CargaPedido")]
        public virtual Pedido Pedido { get; set; }
    }
}
