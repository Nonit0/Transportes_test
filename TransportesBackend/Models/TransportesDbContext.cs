using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace TransportesBackend.Models
{
    public partial class TransportesDbContext : DbContext
    {
        public TransportesDbContext()
        {
        }

        public TransportesDbContext(DbContextOptions<TransportesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Almacen> Almacen { get; set; }
        public virtual DbSet<Camion> Camion { get; set; }
        public virtual DbSet<Carga> Carga { get; set; }
        public virtual DbSet<CargaPedido> CargaPedido { get; set; }
        public virtual DbSet<Cliente> Cliente { get; set; }
        public virtual DbSet<Conductor> Conductor { get; set; }
        public virtual DbSet<Direccion> Direccion { get; set; }
        public virtual DbSet<Entrega> Entrega { get; set; }
        public virtual DbSet<Fabrica> Fabrica { get; set; }
        public virtual DbSet<Pedido> Pedido { get; set; }
        public virtual DbSet<PedidoDetalle> PedidoDetalle { get; set; }
        public virtual DbSet<Producto> Producto { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_general_ci");

            modelBuilder.Entity<Almacen>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.HasOne(d => d.Direccion)
                    .WithMany(p => p.Almacen)
                    .HasForeignKey(d => d.DireccionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_alm_dir");
            });

            modelBuilder.Entity<Camion>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.Property(e => e.Activo).HasDefaultValueSql("'1'");

                entity.Property(e => e.CapacidadPeso).HasPrecision(10, 2);

                entity.Property(e => e.CapacidadVolumen).HasPrecision(10, 2);
            });

            modelBuilder.Entity<Carga>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.HasOne(d => d.Camion)
                    .WithMany(p => p.Carga)
                    .HasForeignKey(d => d.CamionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_carga_cam");

                entity.HasOne(d => d.Conductor)
                    .WithMany(p => p.Carga)
                    .HasForeignKey(d => d.ConductorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_carga_cond");

                entity.HasOne(d => d.DestinoAlmacen)
                    .WithMany(p => p.CargaDestinoAlmacen)
                    .HasForeignKey(d => d.DestinoAlmacenId)
                    .HasConstraintName("fk_carga_dest_alm");

                entity.HasOne(d => d.DestinoCliente)
                    .WithMany(p => p.Carga)
                    .HasForeignKey(d => d.DestinoClienteId)
                    .HasConstraintName("fk_carga_dest_cli");

                entity.HasOne(d => d.OrigenAlmacen)
                    .WithMany(p => p.CargaOrigenAlmacen)
                    .HasForeignKey(d => d.OrigenAlmacenId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_carga_origen");
            });

            modelBuilder.Entity<CargaPedido>(entity =>
            {
                entity.HasKey(e => new { e.CargaId, e.PedidoId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.HasOne(d => d.Carga)
                    .WithMany(p => p.CargaPedido)
                    .HasForeignKey(d => d.CargaId)
                    .HasConstraintName("fk_cp_carga");

                entity.HasOne(d => d.Pedido)
                    .WithMany(p => p.CargaPedido)
                    .HasForeignKey(d => d.PedidoId)
                    .HasConstraintName("fk_cp_pedido");
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.HasOne(d => d.Direccion)
                    .WithMany(p => p.Cliente)
                    .HasForeignKey(d => d.DireccionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_cliente_dir");
            });

            modelBuilder.Entity<Conductor>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");
            });

            modelBuilder.Entity<Direccion>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.Property(e => e.Pais).HasDefaultValueSql("'España'");
            });

            modelBuilder.Entity<Entrega>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.Property(e => e.Estado).HasDefaultValueSql("'Pendiente'");

                entity.HasOne(d => d.Carga)
                    .WithMany(p => p.Entrega)
                    .HasForeignKey(d => d.CargaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ent_carga");

                entity.HasOne(d => d.Pedido)
                    .WithMany(p => p.Entrega)
                    .HasForeignKey(d => d.PedidoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ent_pedido");
            });

            modelBuilder.Entity<Fabrica>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.HasOne(d => d.Direccion)
                    .WithMany(p => p.Fabrica)
                    .HasForeignKey(d => d.DireccionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_fab_dir");
            });

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.Property(e => e.Estado).HasDefaultValueSql("'Pendiente'");

                entity.HasOne(d => d.Cliente)
                    .WithMany(p => p.Pedido)
                    .HasForeignKey(d => d.ClienteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ped_cliente");
            });

            modelBuilder.Entity<PedidoDetalle>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.HasOne(d => d.Pedido)
                    .WithMany(p => p.PedidoDetalle)
                    .HasForeignKey(d => d.PedidoId)
                    .HasConstraintName("fk_pd_pedido");

                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.PedidoDetalle)
                    .HasForeignKey(d => d.ProductoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_pd_producto");
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.Property(e => e.PesoUnitario).HasPrecision(10, 2);

                entity.Property(e => e.VolumenUnitario).HasPrecision(10, 2);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
