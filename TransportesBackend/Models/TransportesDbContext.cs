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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseMySql("server=localhost;database=transportes_db;user=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.32-mariadb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_general_ci");

            modelBuilder.Entity<Almacen>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.HasOne(d => d.Direccion)
                    .WithMany(p => p.Almacens)
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
                    .WithMany(p => p.Cargas)
                    .HasForeignKey(d => d.CamionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_carga_cam");

                entity.HasOne(d => d.Conductor)
                    .WithMany(p => p.Cargas)
                    .HasForeignKey(d => d.ConductorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_carga_cond");

                entity.HasOne(d => d.DestinoAlmacen)
                    .WithMany(p => p.CargaDestinoAlmacens)
                    .HasForeignKey(d => d.DestinoAlmacenId)
                    .HasConstraintName("fk_carga_dest_alm");

                entity.HasOne(d => d.DestinoCliente)
                    .WithMany(p => p.Cargas)
                    .HasForeignKey(d => d.DestinoClienteId)
                    .HasConstraintName("fk_carga_dest_cli");

                entity.HasOne(d => d.OrigenAlmacen)
                    .WithMany(p => p.CargaOrigenAlmacens)
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
                    .WithMany(p => p.CargaPedidos)
                    .HasForeignKey(d => d.CargaId)
                    .HasConstraintName("fk_cp_carga");

                entity.HasOne(d => d.Pedido)
                    .WithMany(p => p.CargaPedidos)
                    .HasForeignKey(d => d.PedidoId)
                    .HasConstraintName("fk_cp_pedido");
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.HasOne(d => d.Direccion)
                    .WithMany(p => p.Clientes)
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
                    .WithMany(p => p.Entregas)
                    .HasForeignKey(d => d.CargaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ent_carga");

                entity.HasOne(d => d.Pedido)
                    .WithMany(p => p.Entregas)
                    .HasForeignKey(d => d.PedidoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ent_pedido");
            });

            modelBuilder.Entity<Fabrica>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.HasOne(d => d.Direccion)
                    .WithMany(p => p.Fabricas)
                    .HasForeignKey(d => d.DireccionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_fab_dir");
            });

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.Property(e => e.Estado).HasDefaultValueSql("'Pendiente'");

                entity.HasOne(d => d.Cliente)
                    .WithMany(p => p.Pedidos)
                    .HasForeignKey(d => d.ClienteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ped_cliente");
            });

            modelBuilder.Entity<PedidoDetalle>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("uuid()");

                entity.HasOne(d => d.Pedido)
                    .WithMany(p => p.PedidoDetalles)
                    .HasForeignKey(d => d.PedidoId)
                    .HasConstraintName("fk_pd_pedido");

                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.PedidoDetalles)
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
