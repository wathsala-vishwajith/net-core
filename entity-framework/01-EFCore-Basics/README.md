# Entity Framework Core Basics

This project demonstrates the fundamental concepts of Entity Framework Core, Microsoft's modern object-database mapper for .NET.

## Overview

Entity Framework Core (EF Core) is a lightweight, extensible, open source, and cross-platform version of the popular Entity Framework data access technology. It serves as an object-relational mapper (ORM) that enables .NET developers to work with a database using .NET objects.

## Project Structure

```
01-EFCore-Basics/
├── Data/
│   └── SchoolContext.cs          # DbContext configuration
├── Models/
│   ├── Student.cs                # Student entity
│   ├── Course.cs                 # Course entity
│   └── Enrollment.cs             # Enrollment entity (junction table)
├── Program.cs                    # Main demonstration code
├── EFCoreBasics.csproj          # Project file
└── README.md                     # This file
```

## Key Concepts Demonstrated

### 1. **DbContext**
The `SchoolContext` class inherits from `DbContext` and represents a session with the database. It:
- Configures database connection (using SQLite)
- Defines `DbSet<T>` properties for entity collections
- Configures entity models using Fluent API in `OnModelCreating`
- Seeds initial data

**Location:** `Data/SchoolContext.cs`

### 2. **Entities (Models)**
Three entity classes represent database tables:
- **Student**: Represents students with properties like FirstName, LastName, Email
- **Course**: Represents courses with Title, Credits, Description
- **Enrollment**: Junction table connecting Students and Courses (many-to-many relationship)

**Location:** `Models/` directory

### 3. **Relationships**
The project demonstrates:
- **One-to-Many**: One Student can have many Enrollments
- **One-to-Many**: One Course can have many Enrollments
- **Many-to-Many**: Students and Courses are related through Enrollments

Configuration is done in `SchoolContext.OnModelCreating()` using Fluent API.

### 4. **CRUD Operations**
`DemonstrateCRUDOperations()` shows:
- **Create**: Adding new entities using `context.Students.Add()`
- **Read**: Querying entities using LINQ (`FirstOrDefault`, etc.)
- **Update**: Modifying entity properties and calling `SaveChanges()`
- **Delete**: Removing entities using `context.Students.Remove()`

**Location:** `Program.cs:35-71`

### 5. **Querying**
`DemonstrateQuerying()` demonstrates:
- Simple queries: `context.Students.ToList()`
- Filtering: `Where()` clauses
- Ordering: `OrderBy()` and `ThenBy()`
- Projection: `Select()` to create anonymous types
- Aggregation: `Count()`, `Average()`, etc.

**Location:** `Program.cs:73-125`

### 6. **Navigation Properties**
`DemonstrateRelationships()` shows:
- Using `Include()` and `ThenInclude()` for eager loading
- Navigating from Students to Enrollments to Courses
- Adding related entities
- Querying with relationship counts

**Location:** `Program.cs:127-182`

## Configuration Highlights

### Entity Configuration (Fluent API)

```csharp
modelBuilder.Entity<Student>(entity =>
{
    entity.HasKey(e => e.StudentId);
    entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
    entity.HasIndex(e => e.Email).IsUnique();
});
```

This configures:
- Primary key
- Required fields and maximum lengths
- Unique indexes

### Relationship Configuration

```csharp
entity.HasOne(e => e.Student)
    .WithMany(s => s.Enrollments)
    .HasForeignKey(e => e.StudentId)
    .OnDelete(DeleteBehavior.Cascade);
```

This defines:
- Foreign key relationships
- Delete behavior (cascade deletes)
- Navigation properties

## Running the Project

1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Run the project:**
   ```bash
   dotnet run
   ```

3. **Expected output:**
   - Database creation confirmation
   - CRUD operations demonstration
   - Query results showing students, courses, and enrollments
   - Relationship navigation examples

## Database

This project uses **SQLite** as the database provider:
- **File**: `school.db` (created automatically)
- **Advantages**: No server setup required, file-based, great for learning
- **Location**: Created in the project's bin directory when run

## NuGet Packages Used

- `Microsoft.EntityFrameworkCore` (7.0.14): Core EF functionality
- `Microsoft.EntityFrameworkCore.SqlServer` (7.0.14): SQL Server provider
- `Microsoft.EntityFrameworkCore.Sqlite` (7.0.14): SQLite provider (used in this example)
- `Microsoft.EntityFrameworkCore.Design` (7.0.14): Design-time tools

## Learn More

- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [DbContext Configuration](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- [Creating a Model](https://learn.microsoft.com/en-us/ef/core/modeling/)
- [Querying Data](https://learn.microsoft.com/en-us/ef/core/querying/)
- [Saving Data](https://learn.microsoft.com/en-us/ef/core/saving/)

## Next Steps

After understanding these basics, explore:
1. **Code First Migrations** (Project 02): Learn how to evolve your database schema
2. **Change Tracker API** (Project 03): Understand how EF tracks entity changes
3. **Loading Strategies** (Project 04): Master lazy, eager, and explicit loading
