namespace EFCoreBasics.Models;

/// <summary>
/// Represents a student entity
/// </summary>
public class Student
{
    public int StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }

    // Navigation property for one-to-many relationship
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
