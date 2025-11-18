namespace ChangeTrackerAPI.Models;

/// <summary>
/// Represents a comment on a blog post
/// </summary>
public class Comment
{
    public int CommentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int BlogPostId { get; set; }

    // Navigation property
    public BlogPost BlogPost { get; set; } = null!;
}
