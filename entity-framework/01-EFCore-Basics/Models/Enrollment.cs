namespace EFCoreBasics.Models;

/// <summary>
/// Represents an enrollment (junction table for many-to-many relationship)
/// </summary>
public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public Grade? Grade { get; set; }
    public DateTime EnrollmentDate { get; set; }

    // Navigation properties
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

/// <summary>
/// Enum representing possible grades
/// </summary>
public enum Grade
{
    A, B, C, D, F
}
