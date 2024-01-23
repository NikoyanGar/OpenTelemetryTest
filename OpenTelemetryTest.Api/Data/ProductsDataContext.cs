using Microsoft.EntityFrameworkCore;

namespace OpenTelemetryTest.Api.Data
{

    public class ProductsDataContext : DbContext
    {
        public ProductsDataContext()
        {
        }

        public ProductsDataContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<Product>()
                .HasData(
                    new Product
                    {
                        Id = 1,
                        Name = "Mechanical Keyboard",
                        Description = "RGB mechanical keyboard with Cherry MX Red switches",
                        Price = 99.99M
                    },
                    new Product
                    {
                        Id = 2,
                        Name = "Gaming Mouse",
                        Description = "RGB gaming mouse with 7 programmable buttons",
                        Price = 69.99M
                    }
                );
        }

    }
}
