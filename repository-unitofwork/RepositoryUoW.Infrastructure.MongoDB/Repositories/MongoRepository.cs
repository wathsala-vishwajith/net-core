using System.Linq.Expressions;
using MongoDB.Driver;
using RepositoryUoW.Core.Interfaces;
using RepositoryUoW.Domain.Common;
using RepositoryUoW.Infrastructure.MongoDB.Data;

namespace RepositoryUoW.Infrastructure.MongoDB.Repositories;

/// <summary>
/// MongoDB implementation of the generic repository pattern
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public class MongoRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly MongoDbContext _context;
    protected readonly IMongoCollection<T> _collection;
    protected IClientSessionHandle? _session;

    public MongoRepository(MongoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _collection = _context.GetCollection<T>();
    }

    public void SetSession(IClientSessionHandle session)
    {
        _session = session;
    }

    protected FilterDefinition<T> NotDeletedFilter =>
        Builders<T>.Filter.Eq(x => x.IsDeleted, false);

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Eq(x => x.Id, id),
            NotDeletedFilter
        );

        return _session != null
            ? await _collection.Find(_session, filter).FirstOrDefaultAsync(cancellationToken)
            : await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _session != null
            ? await _collection.Find(_session, NotDeletedFilter).ToListAsync(cancellationToken)
            : await _collection.Find(NotDeletedFilter).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Where(predicate),
            NotDeletedFilter
        );

        return _session != null
            ? await _collection.Find(_session, filter).ToListAsync(cancellationToken)
            : await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Where(predicate),
            NotDeletedFilter
        );

        return _session != null
            ? await _collection.Find(_session, filter).FirstOrDefaultAsync(cancellationToken)
            : await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        var filterDefinition = filter != null
            ? Builders<T>.Filter.And(Builders<T>.Filter.Where(filter), NotDeletedFilter)
            : NotDeletedFilter;

        var totalCount = _session != null
            ? await _collection.CountDocumentsAsync(_session, filterDefinition, cancellationToken: cancellationToken)
            : await _collection.CountDocumentsAsync(filterDefinition, cancellationToken: cancellationToken);

        var findFluent = _session != null
            ? _collection.Find(_session, filterDefinition)
            : _collection.Find(filterDefinition);

        var items = await findFluent
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, (int)totalCount);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var filter = predicate != null
            ? Builders<T>.Filter.And(Builders<T>.Filter.Where(predicate), NotDeletedFilter)
            : NotDeletedFilter;

        var count = _session != null
            ? await _collection.CountDocumentsAsync(_session, filter, cancellationToken: cancellationToken)
            : await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        return (int)count;
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Where(predicate),
            NotDeletedFilter
        );

        var count = _session != null
            ? await _collection.CountDocumentsAsync(_session, filter, new CountOptions { Limit = 1 }, cancellationToken)
            : await _collection.CountDocumentsAsync(filter, new CountOptions { Limit = 1 }, cancellationToken);

        return count > 0;
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (_session != null)
        {
            await _collection.InsertOneAsync(_session, entity, cancellationToken: cancellationToken);
        }
        else
        {
            await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        }

        return entity;
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();

        if (_session != null)
        {
            await _collection.InsertManyAsync(_session, entityList, cancellationToken: cancellationToken);
        }
        else
        {
            await _collection.InsertManyAsync(entityList, cancellationToken: cancellationToken);
        }

        return entityList;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);

        if (_session != null)
        {
            await _collection.ReplaceOneAsync(_session, filter, entity, cancellationToken: cancellationToken);
        }
        else
        {
            await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);

        if (_session != null)
        {
            await _collection.DeleteOneAsync(_session, filter, cancellationToken);
        }
        else
        {
            await _collection.DeleteOneAsync(filter, cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);

        if (_session != null)
        {
            await _collection.DeleteOneAsync(_session, filter, cancellationToken);
        }
        else
        {
            await _collection.DeleteOneAsync(filter, cancellationToken);
        }
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var ids = entities.Select(e => e.Id);
        var filter = Builders<T>.Filter.In(x => x.Id, ids);

        if (_session != null)
        {
            await _collection.DeleteManyAsync(_session, filter, cancellationToken);
        }
        else
        {
            await _collection.DeleteManyAsync(filter, cancellationToken);
        }
    }

    public virtual async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        var update = Builders<T>.Update
            .Set(x => x.IsDeleted, true)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        if (_session != null)
        {
            await _collection.UpdateOneAsync(_session, filter, update, cancellationToken: cancellationToken);
        }
        else
        {
            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }
    }
}
