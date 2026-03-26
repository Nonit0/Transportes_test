using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("entrega")]
    [Index(nameof(CargaId), Name = "fk_ent_carga")]
    [Index(nameof(PedidoId), Name = "idx_entrega_pedido")]
    public partial class Entrega
    {
        [Key]
        [Column("id")]
        [StringLength(36)]
        public string Id { get; set; }
        [Required]
        [Column("carga_id")]
        [StringLength(36)]
        public string CargaId { get; set; }
        [Required]
        [Column("pedido_id")]
        [StringLength(36)]
        public string PedidoId { get; set; }
        [Column("fecha_entrega", TypeName = "datetime")]
        public DateTime? FechaEntrega { get; set; }
        [Required]
        [Column("estado")]
        [StringLength(30)]
        public string Estado { get; set; }
        [Column("observaciones", TypeName = "text")]
        public string Observaciones { get; set; }

        [ForeignKey(nameof(CargaId))]
        [InverseProperty("Entrega")]
        public virtual Carga Carga { get; set; }
        [ForeignKey(nameof(PedidoId))]
        [InverseProperty("Entrega")]
        public virtual Pedido Pedido { get; set; }
    }
}
