using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepositoryUoW.Domain.Entities;

namespace RepositoryUoW.Infrastructure.SQL.Configurations;

/// <summary>
/// Entity Framework configuration for Order entity
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.ShippingAddress)
            .HasMaxLength(500);

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        builder.Property(o => o.Status)
            .HasConversion<string>();

        builder.HasIndex(o => o.OrderDate);
        builder.HasIndex(o => o.Status);

        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
