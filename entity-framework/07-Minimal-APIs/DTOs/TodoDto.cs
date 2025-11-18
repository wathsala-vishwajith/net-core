namespace MinimalAPIs.DTOs;

/// <summary>
/// DTO for creating a new todo
/// </summary>
public record CreateTodoDto(
    string Title,
    string? Description,
    string Priority,
    string? Category
);

/// <summary>
/// DTO for updating a todo
/// </summary>
public record UpdateTodoDto(
    string? Title,
    string? Description,
    bool? IsCompleted,
    string? Priority,
    string? Category
);

/// <summary>
/// DTO for todo response
/// </summary>
public record TodoResponseDto(
    int Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string Priority,
    string? Category
);
