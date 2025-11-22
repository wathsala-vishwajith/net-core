namespace RepositoryUoW.Infrastructure.MongoDB.Settings;

/// <summary>
/// MongoDB connection and database settings
/// </summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "RepositoryUoWDb";
    public int MaxConnectionPoolSize { get; set; } = 100;
    public int MinConnectionPoolSize { get; set; } = 10;
    public int ConnectionTimeout { get; set; } = 30000; // 30 seconds
}
