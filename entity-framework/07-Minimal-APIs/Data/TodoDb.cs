using Microsoft.EntityFrameworkCore;
using MinimalAPIs.Models;

namespace MinimalAPIs.Data;

public class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options) : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed some initial data
        modelBuilder.Entity<Todo>().HasData(
            new Todo
            {
                Id = 1,
                Title = "Learn Minimal APIs",
                Description = "Study ASP.NET Core Minimal APIs",
                IsCompleted = false,
                Priority = "High",
                Category = "Learning",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Todo
            {
                Id = 2,
                Title = "Build sample project",
                Description = "Create a todo API using minimal APIs",
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow.AddDays(-2),
                Priority = "Medium",
                Category = "Development",
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new Todo
            {
                Id = 3,
                Title = "Write documentation",
                Description = "Document all API endpoints",
                IsCompleted = false,
                Priority = "Medium",
                Category = "Documentation",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        );
    }
}
