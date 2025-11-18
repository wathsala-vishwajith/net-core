namespace MinimalAPIs.Models;

/// <summary>
/// Represents a todo item
/// </summary>
public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string Priority { get; set; } = "Medium"; // Low, Medium, High
    public string? Category { get; set; }
}
