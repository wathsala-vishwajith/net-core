namespace ChangeTrackerAPI.Models;

/// <summary>
/// Represents a tag that can be applied to blog posts
/// </summary>
public class Tag
{
    public int TagId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation property (many-to-many)
    public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}
