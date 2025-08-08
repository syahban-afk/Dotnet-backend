using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfigurasi default untuk UUID
            modelBuilder.Entity<ProductCategory>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Product>()
            .Property(p => p.Id)
            .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Customer>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()");
        }
    }
}