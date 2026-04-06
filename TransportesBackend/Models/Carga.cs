using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    [Table("carga")]
    [Index(nameof(ConductorId), Name = "fk_carga_cond")]
    [Index(nameof(DestinoAlmacenId), Name = "fk_carga_dest_alm")]
    [Index(nameof(DestinoClienteId), Name = "fk_carga_dest_cli")]
    [Index(nameof(OrigenAlmacenId), Name = "fk_carga_origen")]
    [Index(nameof(CamionId), Name = "idx_carga_camion")]
    public partial class Carga
    {
        public Carga()
        {
            CargaPedidos = new HashSet<CargaPedido>();
            Entregas = new HashSet<Entrega>();
        }

        [Key]
        [Column("id")]
        [StringLength(36)]
        public string Id { get; set; }
        [Required]
        [Column("camion_id")]
        [StringLength(36)]
        public string CamionId { get; set; }
        [Required]
        [Column("conductor_id")]
        [StringLength(36)]
        public string ConductorId { get; set; }
        [Required]
        [Column("origen_almacen_id")]
        [StringLength(36)]
        public string OrigenAlmacenId { get; set; }
        [Required]
        [Column("historico_origen_nombre")]
        [StringLength(150)]
        public string HistoricoOrigenNombre { get; set; }
        [Column("destino_almacen_id")]
        [StringLength(36)]
        public string DestinoAlmacenId { get; set; }
        [Column("destino_cliente_id")]
        [StringLength(36)]
        public string DestinoClienteId { get; set; }
        [Required]
        [Column("historico_destino_nombre")]
        [StringLength(150)]
        public string HistoricoDestinoNombre { get; set; }
        [Column("fecha_salida", TypeName = "datetime")]
        public DateTime FechaSalida { get; set; }
        [Column("fecha_llegada_estimada", TypeName = "datetime")]
        public DateTime? FechaLlegadaEstimada { get; set; }
        [Column("deleted_at", TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(CamionId))]
        [InverseProperty("Cargas")]
        public virtual Camion Camion { get; set; }
        [ForeignKey(nameof(ConductorId))]
        [InverseProperty("Cargas")]
        public virtual Conductor Conductor { get; set; }
        [ForeignKey(nameof(DestinoAlmacenId))]
        [InverseProperty(nameof(Almacen.CargaDestinoAlmacens))]
        public virtual Almacen DestinoAlmacen { get; set; }
        [ForeignKey(nameof(DestinoClienteId))]
        [InverseProperty(nameof(Cliente.Cargas))]
        public virtual Cliente DestinoCliente { get; set; }
        [ForeignKey(nameof(OrigenAlmacenId))]
        [InverseProperty(nameof(Almacen.CargaOrigenAlmacens))]
        public virtual Almacen OrigenAlmacen { get; set; }
        [InverseProperty(nameof(CargaPedido.Carga))]
        public virtual ICollection<CargaPedido> CargaPedidos { get; set; }
        [InverseProperty(nameof(Entrega.Carga))]
        public virtual ICollection<Entrega> Entregas { get; set; }
    }
}
