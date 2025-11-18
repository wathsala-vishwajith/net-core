using EFCoreBasics.Data;
using EFCoreBasics.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreBasics;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Entity Framework Core Basics Demo ===\n");

        // Ensure database is created
        using (var context = new SchoolContext())
        {
            // This will create the database and apply seeded data
            context.Database.EnsureCreated();
            Console.WriteLine("Database created/verified.\n");
        }

        // Demonstrate basic CRUD operations
        DemonstrateCRUDOperations();

        // Demonstrate querying
        DemonstrateQuerying();

        // Demonstrate relationships
        DemonstrateRelationships();

        Console.WriteLine("\n=== Demo Complete ===");
    }

    /// <summary>
    /// Demonstrates Create, Read, Update, Delete operations
    /// </summary>
    static void DemonstrateCRUDOperations()
    {
        Console.WriteLine("--- CRUD Operations Demo ---\n");

        using var context = new SchoolContext();

        // CREATE: Add a new student
        var newStudent = new Student
        {
            FirstName = "Alice",
            LastName = "Williams",
            Email = "alice.williams@example.com",
            EnrollmentDate = DateTime.Now
        };

        context.Students.Add(newStudent);
        context.SaveChanges();
        Console.WriteLine($"Created: {newStudent.FirstName} {newStudent.LastName} (ID: {newStudent.StudentId})");

        // READ: Query the student we just created
        var student = context.Students.FirstOrDefault(s => s.Email == "alice.williams@example.com");
        Console.WriteLine($"Read: {student?.FirstName} {student?.LastName}");

        // UPDATE: Modify the student
        if (student != null)
        {
            student.LastName = "Williams-Brown";
            context.SaveChanges();
            Console.WriteLine($"Updated: {student.FirstName} {student.LastName}");
        }

        // DELETE: Remove the student
        if (student != null)
        {
            context.Students.Remove(student);
            context.SaveChanges();
            Console.WriteLine($"Deleted: {student.FirstName} {student.LastName}\n");
        }
    }

    /// <summary>
    /// Demonstrates various querying techniques
    /// </summary>
    static void DemonstrateQuerying()
    {
        Console.WriteLine("--- Querying Demo ---\n");

        using var context = new SchoolContext();

        // Simple query - Get all students
        Console.WriteLine("All Students:");
        var allStudents = context.Students.ToList();
        foreach (var student in allStudents)
        {
            Console.WriteLine($"  - {student.FirstName} {student.LastName} ({student.Email})");
        }

        // Filtered query - Get students enrolled after a specific date
        Console.WriteLine("\nStudents enrolled in 2024:");
        var recentStudents = context.Students
            .Where(s => s.EnrollmentDate.Year == 2024)
            .ToList();
        foreach (var student in recentStudents)
        {
            Console.WriteLine($"  - {student.FirstName} {student.LastName}");
        }

        // Ordering
        Console.WriteLine("\nStudents ordered by last name:");
        var orderedStudents = context.Students
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToList();
        foreach (var student in orderedStudents)
        {
            Console.WriteLine($"  - {student.LastName}, {student.FirstName}");
        }

        // Projection - Select specific fields
        Console.WriteLine("\nStudent emails:");
        var emails = context.Students
            .Select(s => new { s.Email, FullName = $"{s.FirstName} {s.LastName}" })
            .ToList();
        foreach (var item in emails)
        {
            Console.WriteLine($"  - {item.FullName}: {item.Email}");
        }

        // Aggregation
        var studentCount = context.Students.Count();
        var averageCredits = context.Courses.Average(c => c.Credits);
        Console.WriteLine($"\nTotal Students: {studentCount}");
        Console.WriteLine($"Average Course Credits: {averageCredits:F2}\n");
    }

    /// <summary>
    /// Demonstrates working with relationships
    /// </summary>
    static void DemonstrateRelationships()
    {
        Console.WriteLine("--- Relationships Demo ---\n");

        using var context = new SchoolContext();

        // Query with Include (eager loading - covered more in Loading Strategies project)
        Console.WriteLine("Students with their enrollments:");
        var studentsWithEnrollments = context.Students
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
            .ToList();

        foreach (var student in studentsWithEnrollments)
        {
            Console.WriteLine($"\n{student.FirstName} {student.LastName}:");
            foreach (var enrollment in student.Enrollments)
            {
                var gradeStr = enrollment.Grade.HasValue ? enrollment.Grade.ToString() : "In Progress";
                Console.WriteLine($"  - {enrollment.Course.Title} ({enrollment.Course.Credits} credits) - Grade: {gradeStr}");
            }
        }

        // Add a new enrollment
        Console.WriteLine("\n\nAdding new enrollment:");
        var johnDoe = context.Students.First(s => s.FirstName == "John" && s.LastName == "Doe");
        var aspNetCourse = context.Courses.First(c => c.Title == "ASP.NET Core");

        var newEnrollment = new Enrollment
        {
            Student = johnDoe,
            Course = aspNetCourse,
            EnrollmentDate = DateTime.Now
        };

        context.Enrollments.Add(newEnrollment);
        context.SaveChanges();
        Console.WriteLine($"Enrolled {johnDoe.FirstName} {johnDoe.LastName} in {aspNetCourse.Title}");

        // Query courses with student count
        Console.WriteLine("\n\nCourses with enrollment count:");
        var coursesWithCount = context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .ToList();

        foreach (var course in coursesWithCount)
        {
            Console.WriteLine($"  - {course.Title}: {course.EnrollmentCount} students");
        }
    }
}
