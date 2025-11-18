namespace LoadingStrategies.Models;

/// <summary>
/// Represents a book entity
/// </summary>
public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public int Pages { get; set; }
    public int AuthorId { get; set; }

    // Navigation properties - virtual for lazy loading
    public virtual Author Author { get; set; } = null!;
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
