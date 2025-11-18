using Microsoft.EntityFrameworkCore;
using EFCoreBasics.Models;

namespace EFCoreBasics.Data;

/// <summary>
/// Database context for the School database
/// Demonstrates DbContext configuration and DbSet properties
/// </summary>
public class SchoolContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Using SQLite for this example (file-based, no server required)
        optionsBuilder.UseSqlite("Data Source=school.db");

        // Enable sensitive data logging for development
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Student entity
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure Course entity
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Credits).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure Enrollment entity and relationships
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId);

            // Many-to-one relationship: Enrollment -> Student
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-one relationship: Enrollment -> Course
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed students
        modelBuilder.Entity<Student>().HasData(
            new Student { StudentId = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", EnrollmentDate = new DateTime(2023, 9, 1) },
            new Student { StudentId = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", EnrollmentDate = new DateTime(2023, 9, 1) },
            new Student { StudentId = 3, FirstName = "Bob", LastName = "Johnson", Email = "bob.johnson@example.com", EnrollmentDate = new DateTime(2024, 1, 15) }
        );

        // Seed courses
        modelBuilder.Entity<Course>().HasData(
            new Course { CourseId = 1, Title = "Introduction to C#", Credits = 3, Description = "Learn the basics of C# programming" },
            new Course { CourseId = 2, Title = "Entity Framework Core", Credits = 4, Description = "Master EF Core for data access" },
            new Course { CourseId = 3, Title = "ASP.NET Core", Credits = 4, Description = "Build web applications with ASP.NET Core" }
        );

        // Seed enrollments
        modelBuilder.Entity<Enrollment>().HasData(
            new Enrollment { EnrollmentId = 1, StudentId = 1, CourseId = 1, Grade = Grade.A, EnrollmentDate = new DateTime(2023, 9, 5) },
            new Enrollment { EnrollmentId = 2, StudentId = 1, CourseId = 2, Grade = Grade.B, EnrollmentDate = new DateTime(2023, 9, 5) },
            new Enrollment { EnrollmentId = 3, StudentId = 2, CourseId = 1, Grade = Grade.A, EnrollmentDate = new DateTime(2023, 9, 6) },
            new Enrollment { EnrollmentId = 4, StudentId = 2, CourseId = 3, EnrollmentDate = new DateTime(2023, 9, 6) },
            new Enrollment { EnrollmentId = 5, StudentId = 3, CourseId = 2, EnrollmentDate = new DateTime(2024, 1, 20) }
        );
    }
}
