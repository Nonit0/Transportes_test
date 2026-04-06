using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("cliente")]
    [Index(nameof(DireccionId), Name = "fk_cliente_dir")]
    [Index(nameof(DeletedAt), nameof(Nombre), Name = "idx_cliente_activo")]
    public partial class Cliente
    {
        public Cliente()
        {
            Cargas = new HashSet<Carga>();
            Pedidos = new HashSet<Pedido>();
        }

        [Key]
        [Column("id")]
        [StringLength(36)]
        public string Id { get; set; }
        [Required]
        [Column("nombre")]
        [StringLength(150)]
        public string Nombre { get; set; }
        [Required]
        [Column("direccion_id")]
        [StringLength(36)]
        public string DireccionId { get; set; }
        [Column("telefono")]
        [StringLength(20)]
        public string Telefono { get; set; }
        [Column("email")]
        [StringLength(150)]
        public string Email { get; set; }
        [Column("deleted_at", TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(DireccionId))]
        [InverseProperty("Clientes")]
        public virtual Direccion Direccion { get; set; }
        [InverseProperty(nameof(Carga.DestinoCliente))]
        public virtual ICollection<Carga> Cargas { get; set; }
        [InverseProperty(nameof(Pedido.Cliente))]
        public virtual ICollection<Pedido> Pedidos { get; set; }
    }
}
