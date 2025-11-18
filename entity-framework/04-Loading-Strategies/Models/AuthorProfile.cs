namespace LoadingStrategies.Models;

/// <summary>
/// Represents an author's profile (one-to-one relationship)
/// </summary>
public class AuthorProfile
{
    public int AuthorProfileId { get; set; }
    public string Website { get; set; } = string.Empty;
    public string Twitter { get; set; } = string.Empty;
    public string LinkedIn { get; set; } = string.Empty;
    public int YearsExperience { get; set; }
    public int AuthorId { get; set; }

    // Navigation property - virtual for lazy loading
    public virtual Author Author { get; set; } = null!;
}
