using MongoDB.Driver;
using RepositoryUoW.Core.Interfaces;
using RepositoryUoW.Domain.Common;
using RepositoryUoW.Domain.Entities;
using RepositoryUoW.Infrastructure.MongoDB.Data;
using RepositoryUoW.Infrastructure.MongoDB.Repositories;

namespace RepositoryUoW.Infrastructure.MongoDB.UnitOfWork;

/// <summary>
/// MongoDB implementation of the Unit of Work pattern
/// </summary>
public class MongoUnitOfWork : IUnitOfWork
{
    private readonly MongoDbContext _context;
    private IClientSessionHandle? _session;
    private readonly Dictionary<Type, object> _repositories;

    // Lazy-loaded repositories
    private IRepository<Product>? _products;
    private IRepository<Customer>? _customers;
    private IRepository<Order>? _orders;
    private IRepository<OrderItem>? _orderItems;

    public MongoUnitOfWork(MongoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _repositories = new Dictionary<Type, object>();
    }

    public IRepository<Product> Products =>
        _products ??= CreateRepository<Product>();

    public IRepository<Customer> Customers =>
        _customers ??= CreateRepository<Customer>();

    public IRepository<Order> Orders =>
        _orders ??= CreateRepository<Order>();

    public IRepository<OrderItem> OrderItems =>
        _orderItems ??= CreateRepository<OrderItem>();

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);

        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = CreateRepository<T>();
        }

        return (IRepository<T>)_repositories[type];
    }

    private MongoRepository<T> CreateRepository<T>() where T : BaseEntity
    {
        var repository = new MongoRepository<T>(_context);
        if (_session != null)
        {
            repository.SetSession(_session);
        }
        return repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // MongoDB doesn't require explicit save like EF Core
        // Changes are persisted immediately when repository methods are called
        // This is here for interface compatibility
        return Task.FromResult(0);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _session = await _context.StartSessionAsync();
        _session.StartTransaction();

        // Update all existing repositories with the session
        UpdateRepositoriesWithSession();
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session == null)
        {
            throw new InvalidOperationException("No transaction has been started.");
        }

        try
        {
            await _session.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _session.Dispose();
            _session = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session != null)
        {
            await _session.AbortTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }
    }

    private void UpdateRepositoriesWithSession()
    {
        if (_session == null) return;

        foreach (var repo in _repositories.Values.OfType<MongoRepository<BaseEntity>>())
        {
            repo.SetSession(_session);
        }

        // Update specific repositories if they exist
        if (_products is MongoRepository<Product> productsRepo)
            productsRepo.SetSession(_session);

        if (_customers is MongoRepository<Customer> customersRepo)
            customersRepo.SetSession(_session);

        if (_orders is MongoRepository<Order> ordersRepo)
            ordersRepo.SetSession(_session);

        if (_orderItems is MongoRepository<OrderItem> orderItemsRepo)
            orderItemsRepo.SetSession(_session);
    }

    public void Dispose()
    {
        _session?.Dispose();
        GC.SuppressFinalize(this);
    }
}
