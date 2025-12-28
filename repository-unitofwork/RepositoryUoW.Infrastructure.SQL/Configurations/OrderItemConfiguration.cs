using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RepositoryUoW.Domain.Entities;

namespace RepositoryUoW.Infrastructure.SQL.Configurations;

/// <summary>
/// Entity Framework configuration for OrderItem entity
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.Discount)
            .HasPrecision(18, 2);

        builder.Ignore(oi => oi.TotalPrice);
    }
}
