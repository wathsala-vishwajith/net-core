# Loading Strategies in EF Core

This project demonstrates different strategies for loading related data in Entity Framework Core: **Eager Loading**, **Explicit Loading**, **Lazy Loading**, and **Projection**.

## Overview

When working with related entities, EF Core provides multiple strategies to load data. Choosing the right strategy impacts performance, database queries, and application efficiency.

## Project Structure

```
04-Loading-Strategies/
├── Data/
│   └── LibraryContext.cs        # DbContext with lazy loading support
├── Models/
│   ├── Author.cs                # Author entity with virtual properties
│   ├── Book.cs                  # Book entity
│   ├── Review.cs                # Review entity
│   ├── Category.cs              # Category entity
│   └── AuthorProfile.cs         # One-to-one relationship example
├── Program.cs                   # Comprehensive loading strategy demos
├── LoadingStrategies.csproj    # Project file (includes Proxies package)
└── README.md                    # This file
```

## Loading Strategies

### 1. Eager Loading

**Load related data as part of the initial query using `Include()` and `ThenInclude()`**

#### Basic Usage

```csharp
// Load authors with their books
var authors = context.Authors
    .Include(a => a.Books)
    .ToList();
```

#### Multiple Levels

```csharp
// Load books with author, reviews, and categories
var books = context.Books
    .Include(b => b.Author)
    .Include(b => b.Reviews)
    .Include(b => b.Categories)
    .ToList();
```

#### Nested Includes

```csharp
// Load authors with books and each book's reviews
var authors = context.Authors
    .Include(a => a.Books)
        .ThenInclude(b => b.Reviews)
    .ToList();
```

#### Filtered Include (EF Core 5+)

```csharp
// Load books with only high-rated reviews
var books = context.Books
    .Include(b => b.Reviews.Where(r => r.Rating >= 4))
    .ToList();
```

**Pros:**
- ✅ Single database query (or minimal queries)
- ✅ No N+1 query problem
- ✅ All data loaded upfront

**Cons:**
- ❌ May load unnecessary data
- ❌ Can create large result sets
- ❌ Cartesian explosion with multiple collections

**When to Use:**
- You know you'll need the related data
- Working with small to medium datasets
- Avoiding N+1 queries is critical

**Location:** `Program.cs:88-164`

---

### 2. Explicit Loading

**Manually load related data on demand**

#### Load Collection

```csharp
var author = context.Authors.First();

// Explicitly load the Books collection
context.Entry(author).Collection(a => a.Books).Load();
```

#### Load Reference

```csharp
// Explicitly load the Profile reference (one-to-one)
context.Entry(author).Reference(a => a.Profile).Load();
```

#### Query Related Data

```csharp
// Load with filtering
var recentBooksCount = context.Entry(author)
    .Collection(a => a.Books)
    .Query()
    .Where(b => b.PublishedDate.Year >= 2010)
    .Count();
```

#### Conditional Loading

```csharp
// Load only if not already loaded
if (!context.Entry(author).Collection(a => a.Books).IsLoaded)
{
    context.Entry(author).Collection(a => a.Books).Load();
}
```

**Pros:**
- ✅ Full control over what's loaded
- ✅ Load only when needed
- ✅ Can filter related data before loading

**Cons:**
- ❌ Requires manual loading code
- ❌ Multiple database queries
- ❌ More verbose

**When to Use:**
- Need fine-grained control
- Loading related data conditionally
- Want to filter before loading

**Location:** `Program.cs:166-220`

---

### 3. Lazy Loading

**Automatically load related data when accessed**

#### Setup Requirements

1. **Install Package:**
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.Proxies
   ```

2. **Enable in DbContext:**
   ```csharp
   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
       optionsBuilder.UseLazyLoadingProxies();
   }
   ```

3. **Make Navigation Properties Virtual:**
   ```csharp
   public class Author
   {
       public virtual ICollection<Book> Books { get; set; }
       public virtual AuthorProfile Profile { get; set; }
   }
   ```

#### Usage

```csharp
var author = context.Authors.First();

// Accessing Books triggers automatic loading
Console.WriteLine($"Books: {author.Books.Count}"); // DB query here

// Accessing each book's reviews also triggers loading
foreach (var book in author.Books)
{
    Console.WriteLine($"Reviews: {book.Reviews.Count}"); // DB query per book
}
```

**Pros:**
- ✅ Convenient and automatic
- ✅ Loads data as needed
- ✅ No Include() required

**Cons:**
- ❌ N+1 query problem
- ❌ Requires virtual properties
- ❌ Requires Proxies package
- ❌ Doesn't work with async navigation
- ❌ Performance issues with loops

**When to Use:**
- Rapid prototyping
- Desktop/WinForms applications
- When convenience outweighs performance

**When NOT to Use:**
- Web APIs (performance critical)
- Iterating over collections
- When controlling queries is important

**Location:** `Program.cs:222-254`

---

### 4. Select Loading (Projection)

**Load only specific properties using `Select()`**

#### Project to Anonymous Type

```csharp
var authorSummaries = context.Authors
    .Select(a => new
    {
        FullName = $"{a.FirstName} {a.LastName}",
        BookCount = a.Books.Count,
        BookTitles = a.Books.Select(b => b.Title).ToList()
    })
    .ToList();
```

#### Project with Calculations

```csharp
var bookStats = context.Books
    .Select(b => new
    {
        b.Title,
        Author = $"{b.Author.FirstName} {b.Author.LastName}",
        ReviewCount = b.Reviews.Count,
        AverageRating = b.Reviews.Average(r => r.Rating)
    })
    .ToList();
```

#### Project to DTO

```csharp
public class AuthorDto
{
    public string FullName { get; set; }
    public List<string> BookTitles { get; set; }
}

var dtos = context.Authors
    .Select(a => new AuthorDto
    {
        FullName = $"{a.FirstName} {a.LastName}",
        BookTitles = a.Books.Select(b => b.Title).ToList()
    })
    .ToList();
```

**Pros:**
- ✅ Most efficient - minimal data transfer
- ✅ Single query with JOIN
- ✅ Load only what you need
- ✅ Great for APIs and reports

**Cons:**
- ❌ Returns DTOs, not tracked entities
- ❌ Cannot update returned objects
- ❌ Requires mapping code

**When to Use:**
- Read-only scenarios
- API responses
- Reports and dashboards
- Performance is critical

**Location:** `Program.cs:256-307`

---

## Performance Comparison

Based on the demo (100 iterations):

| Strategy | Performance | Queries | Best For |
|----------|------------|---------|----------|
| **Eager Loading** | Medium | 1-2 queries | Loading known related data |
| **Explicit Loading** | Slow | Multiple queries | Conditional loading |
| **Lazy Loading** | Slowest | N+1 queries | Convenience over performance |
| **Select/Projection** | **Fastest** | 1 query | Read-only, specific data |

**Location:** `Program.cs:309-370`

## The N+1 Query Problem

### What is it?

Loading a collection of entities (1 query) then loading related data for each entity (N queries):

```csharp
// 1 query: Load all authors
var authors = context.Authors.ToList();

// N queries: One per author to load books
foreach (var author in authors)
{
    Console.WriteLine($"Books: {author.Books.Count}"); // Lazy load
}
```

**Result:** 1 + N queries (if 10 authors = 11 queries!)

### How to Avoid

✅ **Use Eager Loading:**
```csharp
var authors = context.Authors
    .Include(a => a.Books)
    .ToList(); // 1 query
```

✅ **Use Projection:**
```csharp
var summaries = context.Authors
    .Select(a => new { a.FirstName, BookCount = a.Books.Count })
    .ToList(); // 1 query
```

## Choosing the Right Strategy

### Decision Tree

```
Are you updating the entities?
├─ Yes → Use Eager or Explicit Loading
└─ No (read-only)
   └─ Use Select/Projection

Need all related data upfront?
├─ Yes → Use Eager Loading
└─ No
   ├─ Need some related data conditionally?
   │  └─ Use Explicit Loading
   └─ Want automatic loading?
      └─ Use Lazy Loading (with caution)
```

### Recommendations

| Scenario | Strategy |
|----------|----------|
| Web API GET endpoints | Select/Projection |
| Updating entities | Eager Loading |
| Conditional related data | Explicit Loading |
| Desktop apps | Lazy Loading (acceptable) |
| Reports/Dashboards | Select/Projection |
| Admin CRUD operations | Eager Loading |

## Best Practices

### 1. Avoid Lazy Loading in Web APIs

```csharp
// ❌ Bad: Lazy loading in API
public IActionResult GetAuthors()
{
    var authors = _context.Authors.ToList();
    return Ok(authors); // May trigger N+1 queries during serialization
}

// ✅ Good: Eager loading or projection
public IActionResult GetAuthors()
{
    var authors = _context.Authors
        .Select(a => new AuthorDto
        {
            FullName = $"{a.FirstName} {a.LastName}",
            Books = a.Books.Select(b => b.Title).ToList()
        })
        .ToList();
    return Ok(authors);
}
```

### 2. Use AsNoTracking for Read-Only Queries

```csharp
var books = context.Books
    .Include(b => b.Author)
    .AsNoTracking()
    .ToList();
```

### 3. Be Careful with Multiple Includes

```csharp
// ⚠️ Can create cartesian explosion
var authors = context.Authors
    .Include(a => a.Books)
    .Include(a => a.Reviews) // Multiple collections!
    .ToList();

// ✅ Better: Use separate queries or split queries
var authors = context.Authors
    .Include(a => a.Books)
    .AsSplitQuery() // EF Core 5+
    .ToList();
```

### 4. Use Filtered Includes

```csharp
// Load only active books
var authors = context.Authors
    .Include(a => a.Books.Where(b => b.IsActive))
    .ToList();
```

## Running This Project

1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

The demo will:
- Show all four loading strategies in action
- Demonstrate the N+1 query problem
- Compare performance of each strategy
- Display SQL queries generated

## Learn More

- [Loading Related Data](https://learn.microsoft.com/en-us/ef/core/querying/related-data/)
- [Eager Loading](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager)
- [Explicit Loading](https://learn.microsoft.com/en-us/ef/core/querying/related-data/explicit)
- [Lazy Loading](https://learn.microsoft.com/en-us/ef/core/querying/related-data/lazy)
- [Split Queries](https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries)

## Related Projects

- **01-EFCore-Basics**: EF Core fundamentals
- **02-CodeFirst-Migrations**: Database schema evolution
- **03-ChangeTracker-API**: Understanding change tracking
