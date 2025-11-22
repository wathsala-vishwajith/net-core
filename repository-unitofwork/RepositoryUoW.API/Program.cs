using Microsoft.EntityFrameworkCore;
using RepositoryUoW.Core.Interfaces;
using RepositoryUoW.Infrastructure.MongoDB.Data;
using RepositoryUoW.Infrastructure.MongoDB.Settings;
using RepositoryUoW.Infrastructure.MongoDB.UnitOfWork;
using RepositoryUoW.Infrastructure.SQL.Data;
using RepositoryUoW.Infrastructure.SQL.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Repository & Unit of Work Pattern API",
        Version = "v1",
        Description = "Demonstrates Repository and Unit of Work patterns with both SQL (EF Core) and NoSQL (MongoDB) implementations"
    });
});

// Configure SQL Server with EF Core (using In-Memory for demo)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("RepositoryUoWDb");
    options.EnableSensitiveDataLogging();
});

// Register SQL Unit of Work
builder.Services.AddScoped<IUnitOfWork>(serviceProvider =>
{
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    return new EfUnitOfWork(context);
});

// Configure MongoDB
var mongoSettings = new MongoDbSettings();
builder.Configuration.GetSection("MongoDbSettings").Bind(mongoSettings);
builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton<MongoDbContext>();

// Register MongoDB Unit of Work as named service
builder.Services.AddScoped<MongoUnitOfWork>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Repository & UoW API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Seed sample data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var sqlUnitOfWork = services.GetRequiredService<IUnitOfWork>();
        await SeedDataAsync(sqlUnitOfWork);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

static async Task SeedDataAsync(IUnitOfWork unitOfWork)
{
    // Check if data already exists
    var existingProducts = await unitOfWork.Products.CountAsync();
    if (existingProducts > 0) return;

    // Seed Products
    var products = new[]
    {
        new RepositoryUoW.Domain.Entities.Product
        {
            Name = "Laptop",
            Description = "High-performance laptop",
            Price = 1299.99m,
            StockQuantity = 50,
            Category = "Electronics",
            SKU = "LAP-001"
        },
        new RepositoryUoW.Domain.Entities.Product
        {
            Name = "Wireless Mouse",
            Description = "Ergonomic wireless mouse",
            Price = 29.99m,
            StockQuantity = 200,
            Category = "Electronics",
            SKU = "MOU-001"
        },
        new RepositoryUoW.Domain.Entities.Product
        {
            Name = "Office Chair",
            Description = "Comfortable office chair",
            Price = 199.99m,
            StockQuantity = 30,
            Category = "Furniture",
            SKU = "CHR-001"
        }
    };

    await unitOfWork.Products.AddRangeAsync(products);

    // Seed Customers
    var customers = new[]
    {
        new RepositoryUoW.Domain.Entities.Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "+1234567890",
            Address = "123 Main St",
            City = "New York",
            Country = "USA"
        },
        new RepositoryUoW.Domain.Entities.Customer
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Phone = "+1234567891",
            Address = "456 Oak Ave",
            City = "Los Angeles",
            Country = "USA"
        }
    };

    await unitOfWork.Customers.AddRangeAsync(customers);
    await unitOfWork.SaveChangesAsync();
}
