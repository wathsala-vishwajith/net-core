namespace LoadingStrategies.Models;

/// <summary>
/// Represents a book category
/// </summary>
public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation property - virtual for lazy loading
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
