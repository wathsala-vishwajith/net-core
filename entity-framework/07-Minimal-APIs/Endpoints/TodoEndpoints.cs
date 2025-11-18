using Microsoft.EntityFrameworkCore;
using MinimalAPIs.Data;
using MinimalAPIs.DTOs;
using MinimalAPIs.Models;

namespace MinimalAPIs.Endpoints;

/// <summary>
/// Defines all Todo-related endpoints using extension methods
/// This demonstrates endpoint organization in Minimal APIs
/// </summary>
public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/todos")
            .WithTags("Todos")
            .WithOpenApi();

        // GET /api/todos - Get all todos
        group.MapGet("/", GetAllTodos)
            .WithName("GetAllTodos")
            .WithSummary("Get all todos")
            .WithDescription("Retrieves all todo items with optional filtering");

        // GET /api/todos/{id} - Get todo by ID
        group.MapGet("/{id:int}", GetTodoById)
            .WithName("GetTodoById")
            .WithSummary("Get todo by ID")
            .Produces<TodoResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/todos - Create new todo
        group.MapPost("/", CreateTodo)
            .WithName("CreateTodo")
            .WithSummary("Create a new todo")
            .Produces<TodoResponseDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        // PUT /api/todos/{id} - Update todo
        group.MapPut("/{id:int}", UpdateTodo)
            .WithName("UpdateTodo")
            .WithSummary("Update an existing todo")
            .Produces<TodoResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // DELETE /api/todos/{id} - Delete todo
        group.MapDelete("/{id:int}", DeleteTodo)
            .WithName("DeleteTodo")
            .WithSummary("Delete a todo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        // GET /api/todos/completed - Get completed todos
        group.MapGet("/completed", GetCompletedTodos)
            .WithName("GetCompletedTodos")
            .WithSummary("Get all completed todos");

        // GET /api/todos/pending - Get pending todos
        group.MapGet("/pending", GetPendingTodos)
            .WithName("GetPendingTodos")
            .WithSummary("Get all pending todos");

        // POST /api/todos/{id}/complete - Mark todo as complete
        group.MapPost("/{id:int}/complete", CompleteTodo)
            .WithName("CompleteTodo")
            .WithSummary("Mark a todo as completed");

        // GET /api/todos/stats - Get statistics
        group.MapGet("/stats", GetStats)
            .WithName("GetStats")
            .WithSummary("Get todo statistics");
    }

    // Handler methods

    static async Task<IResult> GetAllTodos(
        TodoDb db,
        string? category = null,
        string? priority = null,
        bool? isCompleted = null)
    {
        var query = db.Todos.AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(t => t.Category == category);

        if (!string.IsNullOrEmpty(priority))
            query = query.Where(t => t.Priority == priority);

        if (isCompleted.HasValue)
            query = query.Where(t => t.IsCompleted == isCompleted.Value);

        var todos = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapToDto(t))
            .ToListAsync();

        return Results.Ok(todos);
    }

    static async Task<IResult> GetTodoById(int id, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);

        return todo is null
            ? Results.NotFound(new { message = $"Todo with ID {id} not found" })
            : Results.Ok(MapToDto(todo));
    }

    static async Task<IResult> CreateTodo(CreateTodoDto dto, TodoDb db)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return Results.BadRequest(new { message = "Title is required" });
        }

        var todo = new Todo
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Category = dto.Category,
            CreatedAt = DateTime.UtcNow
        };

        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        var response = MapToDto(todo);
        return Results.Created($"/api/todos/{todo.Id}", response);
    }

    static async Task<IResult> UpdateTodo(int id, UpdateTodoDto dto, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null)
            return Results.NotFound(new { message = $"Todo with ID {id} not found" });

        // Update only provided fields
        if (dto.Title is not null)
            todo.Title = dto.Title;

        if (dto.Description is not null)
            todo.Description = dto.Description;

        if (dto.IsCompleted.HasValue)
        {
            todo.IsCompleted = dto.IsCompleted.Value;
            if (dto.IsCompleted.Value && todo.CompletedAt is null)
                todo.CompletedAt = DateTime.UtcNow;
            else if (!dto.IsCompleted.Value)
                todo.CompletedAt = null;
        }

        if (dto.Priority is not null)
            todo.Priority = dto.Priority;

        if (dto.Category is not null)
            todo.Category = dto.Category;

        await db.SaveChangesAsync();

        return Results.Ok(MapToDto(todo));
    }

    static async Task<IResult> DeleteTodo(int id, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null)
            return Results.NotFound(new { message = $"Todo with ID {id} not found" });

        db.Todos.Remove(todo);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    static async Task<IResult> GetCompletedTodos(TodoDb db)
    {
        var todos = await db.Todos
            .Where(t => t.IsCompleted)
            .OrderByDescending(t => t.CompletedAt)
            .Select(t => MapToDto(t))
            .ToListAsync();

        return Results.Ok(todos);
    }

    static async Task<IResult> GetPendingTodos(TodoDb db)
    {
        var todos = await db.Todos
            .Where(t => !t.IsCompleted)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapToDto(t))
            .ToListAsync();

        return Results.Ok(todos);
    }

    static async Task<IResult> CompleteTodo(int id, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null)
            return Results.NotFound(new { message = $"Todo with ID {id} not found" });

        todo.IsCompleted = true;
        todo.CompletedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Results.Ok(MapToDto(todo));
    }

    static async Task<IResult> GetStats(TodoDb db)
    {
        var total = await db.Todos.CountAsync();
        var completed = await db.Todos.CountAsync(t => t.IsCompleted);
        var pending = total - completed;

        var byPriority = await db.Todos
            .GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync();

        var byCategory = await db.Todos
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        var stats = new
        {
            Total = total,
            Completed = completed,
            Pending = pending,
            CompletionRate = total > 0 ? (completed / (double)total * 100) : 0,
            ByPriority = byPriority,
            ByCategory = byCategory
        };

        return Results.Ok(stats);
    }

    // Helper method to map entity to DTO
    private static TodoResponseDto MapToDto(Todo todo) => new(
        todo.Id,
        todo.Title,
        todo.Description,
        todo.IsCompleted,
        todo.CreatedAt,
        todo.CompletedAt,
        todo.Priority,
        todo.Category
    );
}
