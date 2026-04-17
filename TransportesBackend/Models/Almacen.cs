using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("almacen")]
    [Index(nameof(DireccionId), Name = "fk_alm_dir")]
    [Index(nameof(DeletedAt), nameof(Nombre), Name = "idx_almacen_activo")]
    [Index(nameof(Nombre), Name = "nombre", IsUnique = true)]
    public partial class Almacen
    {
        public Almacen()
        {
            CargaDestinoAlmacens = new HashSet<Carga>();
            CargaOrigenAlmacens = new HashSet<Carga>();
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
        [Column("cliente_id")]
        [StringLength(36)]
        public string ClienteId { get; set; }
        [Column("deleted_at", TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(ClienteId))]
        [InverseProperty("Almacenes")]
        public virtual Cliente Cliente { get; set; }
        [ForeignKey(nameof(DireccionId))]
        [InverseProperty("Almacens")]
        public virtual Direccion Direccion { get; set; }
        [InverseProperty(nameof(Carga.DestinoAlmacen))]
        public virtual ICollection<Carga> CargaDestinoAlmacens { get; set; }
        [InverseProperty(nameof(Carga.OrigenAlmacen))]
        public virtual ICollection<Carga> CargaOrigenAlmacens { get; set; }
    }
}
