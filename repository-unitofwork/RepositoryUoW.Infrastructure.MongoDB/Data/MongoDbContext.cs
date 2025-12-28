using MongoDB.Driver;
using RepositoryUoW.Infrastructure.MongoDB.Settings;

namespace RepositoryUoW.Infrastructure.MongoDB.Data;

/// <summary>
/// MongoDB context for managing database operations
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoClient _client;

    public MongoDbContext(MongoDbSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
        clientSettings.MaxConnectionPoolSize = settings.MaxConnectionPoolSize;
        clientSettings.MinConnectionPoolSize = settings.MinConnectionPoolSize;
        clientSettings.ConnectTimeout = TimeSpan.FromMilliseconds(settings.ConnectionTimeout);

        _client = new MongoClient(clientSettings);
        _database = _client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string? collectionName = null)
    {
        return _database.GetCollection<T>(collectionName ?? typeof(T).Name);
    }

    public IMongoDatabase Database => _database;

    public IClientSessionHandle StartSession()
    {
        return _client.StartSession();
    }

    public async Task<IClientSessionHandle> StartSessionAsync()
    {
        return await _client.StartSessionAsync();
    }
}
