using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RepositoryUoW.Core.Interfaces;
using RepositoryUoW.Domain.Common;
using RepositoryUoW.Domain.Entities;
using RepositoryUoW.Infrastructure.SQL.Data;
using RepositoryUoW.Infrastructure.SQL.Repositories;

namespace RepositoryUoW.Infrastructure.SQL.UnitOfWork;

/// <summary>
/// Entity Framework implementation of the Unit of Work pattern
/// </summary>
public class EfUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly Dictionary<Type, object> _repositories;

    // Lazy-loaded repositories
    private IRepository<Product>? _products;
    private IRepository<Customer>? _customers;
    private IRepository<Order>? _orders;
    private IRepository<OrderItem>? _orderItems;

    public EfUnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _repositories = new Dictionary<Type, object>();
    }

    public IRepository<Product> Products =>
        _products ??= new EfRepository<Product>(_context);

    public IRepository<Customer> Customers =>
        _customers ??= new EfRepository<Customer>(_context);

    public IRepository<Order> Orders =>
        _orders ??= new EfRepository<Order>(_context);

    public IRepository<OrderItem> OrderItems =>
        _orderItems ??= new EfRepository<OrderItem>(_context);

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);

        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new EfRepository<T>(_context);
        }

        return (IRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction has been started.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
