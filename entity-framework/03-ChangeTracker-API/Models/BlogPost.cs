namespace ChangeTrackerAPI.Models;

/// <summary>
/// Represents a blog post entity
/// </summary>
public class BlogPost
{
    public int BlogPostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int ViewCount { get; set; }
    public bool IsPublished { get; set; }

    // Navigation properties
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
