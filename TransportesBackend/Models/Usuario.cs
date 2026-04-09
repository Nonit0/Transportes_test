using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TransportesBackend.Models
{
    // El enum DEBE estar aquí para que la clase lo vea
    public enum RolUsuario
    {
        Administrador = 0,
        Operario = 1,
        Cliente = 2
    }
    
    [Table("usuario")]
    [Index(nameof(Email), Name = "email", IsUnique = true)]
    [Index(nameof(ClienteId), Name = "fk_usuario_cliente")]
    public partial class Usuario
    {
        [Key]
        [Column("id")]
        [StringLength(36)]
        public string Id { get; set; }
        [Required]
        [Column("nombre")]
        [StringLength(150)]
        public string Nombre { get; set; }
        [Required]
        [Column("email")]
        [StringLength(150)]
        public string Email { get; set; }
        [Required]
        [Column("password")]
        [StringLength(255)]
        public string Password { get; set; }
        [Column("rol", TypeName = "int(11)")]
        public RolUsuario Rol { get; set; }
        [Column("cliente_id")]
        [StringLength(36)]
        public string ClienteId { get; set; }
        [Column("deleted_at", TypeName = "datetime")]
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(ClienteId))]
        [InverseProperty("Usuario")]
        public virtual Cliente Cliente { get; set; }
    }
}
