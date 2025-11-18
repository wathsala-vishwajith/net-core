namespace EFCoreBasics.Models;

/// <summary>
/// Represents a course entity
/// </summary>
public class Course
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string Description { get; set; } = string.Empty;

    // Navigation property for one-to-many relationship
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
