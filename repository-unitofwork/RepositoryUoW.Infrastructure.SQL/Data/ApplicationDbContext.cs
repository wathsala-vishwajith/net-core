using Microsoft.EntityFrameworkCore;
using RepositoryUoW.Domain.Entities;

namespace RepositoryUoW.Infrastructure.SQL.Data;

/// <summary>
/// Entity Framework Core DbContext for SQL database
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(oi => !oi.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is Domain.Common.BaseEntity entity)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
