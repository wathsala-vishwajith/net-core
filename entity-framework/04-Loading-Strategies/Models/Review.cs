namespace LoadingStrategies.Models;

/// <summary>
/// Represents a book review
/// </summary>
public class Review
{
    public int ReviewId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; }
    public int BookId { get; set; }

    // Navigation property - virtual for lazy loading
    public virtual Book Book { get; set; } = null!;
}
