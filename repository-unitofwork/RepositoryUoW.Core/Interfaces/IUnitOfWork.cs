using RepositoryUoW.Domain.Entities;

namespace RepositoryUoW.Core.Interfaces;

/// <summary>
/// Unit of Work interface for managing transactions across multiple repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repository properties for specific entities
    IRepository<Product> Products { get; }
    IRepository<Customer> Customers { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }

    // Transaction methods
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    // Generic repository access for other entities
    IRepository<T> Repository<T>() where T : Domain.Common.BaseEntity;
}
