using Microsoft.EntityFrameworkCore;
using SalesInvoiceSystem.Models;

namespace SalesInvoiceSystem.Data

{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Sale> Sales { get; set; }

        public DbSet<SaleDetail> SaleDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(x => x.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Sale>()
                .Property(x => x.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SaleDetail>()
                .Property(x => x.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SaleDetail>()
                .Property(x => x.TotalPrice)
                .HasColumnType("decimal(18,2)");

            // Customer -> Sales
            modelBuilder.Entity<Sale>()
                .HasOne(x => x.Customer)
                .WithMany(x => x.Sales)
                .HasForeignKey(x => x.CustomerId);

            // Sale -> SaleDetails
            modelBuilder.Entity<SaleDetail>()
                .HasOne(x => x.Sale)
                .WithMany(x => x.SaleDetails)
                .HasForeignKey(x => x.SaleId);

            // Product -> SaleDetails
            modelBuilder.Entity<SaleDetail>()
                .HasOne(x => x.Product)
                .WithMany(x => x.SaleDetails)
                .HasForeignKey(x => x.ProductId);
        }


    }
}