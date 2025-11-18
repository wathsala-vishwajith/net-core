namespace LoadingStrategies.Models;

/// <summary>
/// Represents an author entity
/// </summary>
public class Author
{
    public int AuthorId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;

    // Navigation properties - virtual for lazy loading
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    public virtual AuthorProfile? Profile { get; set; }
}
