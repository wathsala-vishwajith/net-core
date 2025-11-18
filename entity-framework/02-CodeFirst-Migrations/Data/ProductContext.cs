using Microsoft.EntityFrameworkCore;
using CodeFirstMigrations.Models;

namespace CodeFirstMigrations.Data;

/// <summary>
/// DbContext for Product database
/// Demonstrates Code First approach with migrations
/// </summary>
public class ProductContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }

    public ProductContext() { }

    public ProductContext(DbContextOptions<ProductContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=products.db");
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Price)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.Category)
                .HasMaxLength(50);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Category);
        });

        // Configure Supplier entity
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId);

            entity.Property(e => e.CompanyName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ContactName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Phone)
                .HasMaxLength(20);

            entity.Property(e => e.Address)
                .HasMaxLength(200);

            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure relationship
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Supplier>().HasData(
            new Supplier
            {
                SupplierId = 1,
                CompanyName = "Tech Supplies Inc",
                ContactName = "Sarah Johnson",
                Email = "sarah@techsupplies.com",
                Phone = "555-0101",
                Address = "123 Tech Street, Silicon Valley, CA"
            },
            new Supplier
            {
                SupplierId = 2,
                CompanyName = "Global Electronics",
                ContactName = "Mike Chen",
                Email = "mike@globalelectronics.com",
                Phone = "555-0102",
                Address = "456 Electronic Ave, Austin, TX"
            }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                ProductId = 1,
                Name = "Laptop",
                Price = 999.99m,
                StockQuantity = 50,
                Description = "High-performance laptop",
                Category = "Electronics",
                CreatedDate = DateTime.Now.AddMonths(-6),
                IsActive = true,
                SupplierId = 1
            },
            new Product
            {
                ProductId = 2,
                Name = "Mouse",
                Price = 29.99m,
                StockQuantity = 200,
                Description = "Wireless optical mouse",
                Category = "Accessories",
                CreatedDate = DateTime.Now.AddMonths(-3),
                IsActive = true,
                SupplierId = 1
            },
            new Product
            {
                ProductId = 3,
                Name = "Keyboard",
                Price = 79.99m,
                StockQuantity = 150,
                Description = "Mechanical gaming keyboard",
                Category = "Accessories",
                CreatedDate = DateTime.Now.AddMonths(-2),
                IsActive = true,
                SupplierId = 2
            }
        );
    }
}
