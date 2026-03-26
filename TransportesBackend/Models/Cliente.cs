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
    public partial class Cliente
    {
        public Cliente()
        {
            Carga = new HashSet<Carga>();
            Pedido = new HashSet<Pedido>();
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

        [ForeignKey(nameof(DireccionId))]
        [InverseProperty("Cliente")]
        public virtual Direccion Direccion { get; set; }
        [InverseProperty("DestinoCliente")]
        public virtual ICollection<Carga> Carga { get; set; }
        [InverseProperty("Cliente")]
        public virtual ICollection<Pedido> Pedido { get; set; }
    }
}
