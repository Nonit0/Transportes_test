using Microsoft.EntityFrameworkCore;

namespace TransportesBackend.Models
{
    public partial class TransportesDbContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Producto>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Direccion>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Fabrica>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Almacen>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Cliente>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Camion>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Conductor>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Pedido>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Carga>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Entrega>().HasQueryFilter(e => e.DeletedAt == null);
        }
    }
}
