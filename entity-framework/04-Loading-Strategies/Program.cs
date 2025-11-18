using LoadingStrategies.Data;
using LoadingStrategies.Models;
using Microsoft.EntityFrameworkCore;

namespace LoadingStrategies;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== EF Core Loading Strategies Demo ===\n");

        // Initialize database
        using (var context = new LibraryContext())
        {
            context.Database.EnsureCreated();

            // Seed many-to-many relationships (not supported in HasData)
            SeedManyToManyRelationships(context);
        }

        // Demonstrate Eager Loading
        DemonstrateEagerLoading();

        // Demonstrate Explicit Loading
        DemonstrateExplicitLoading();

        // Demonstrate Lazy Loading
        DemonstrateLazyLoading();

        // Demonstrate Select Loading
        DemonstrateSelectLoading();

        // Performance Comparison
        PerformanceComparison();

        Console.WriteLine("\n=== Demo Complete ===");
    }

    static void SeedManyToManyRelationships(LibraryContext context)
    {
        // Check if already seeded
        if (context.Books.Any(b => b.Categories.Any()))
            return;

        var book1 = context.Books.Find(1);
        var book2 = context.Books.Find(2);
        var book3 = context.Books.Find(3);
        var book4 = context.Books.Find(4);
        var book5 = context.Books.Find(5);

        var cat1 = context.Categories.Find(1);
        var cat2 = context.Categories.Find(2);
        var cat3 = context.Categories.Find(3);
        var cat4 = context.Categories.Find(4);

        if (book1 != null && cat1 != null && cat4 != null)
        {
            book1.Categories.Add(cat1);
            book1.Categories.Add(cat4);
        }

        if (book2 != null && cat3 != null && cat4 != null)
        {
            book2.Categories.Add(cat3);
            book2.Categories.Add(cat4);
        }

        if (book3 != null && cat1 != null && cat4 != null)
        {
            book3.Categories.Add(cat1);
            book3.Categories.Add(cat4);
        }

        if (book4 != null && cat2 != null && cat3 != null)
        {
            book4.Categories.Add(cat2);
            book4.Categories.Add(cat3);
        }

        if (book5 != null && cat2 != null && cat3 != null)
        {
            book5.Categories.Add(cat2);
            book5.Categories.Add(cat3);
        }

        context.SaveChanges();
    }

    /// <summary>
    /// Demonstrates Eager Loading using Include() and ThenInclude()
    /// Loads related data as part of the initial query
    /// </summary>
    static void DemonstrateEagerLoading()
    {
        Console.WriteLine("--- EAGER LOADING ---\n");
        Console.WriteLine("Loads related data in the initial query using Include()");
        Console.WriteLine("Pros: Single database query, no N+1 problem");
        Console.WriteLine("Cons: May load unnecessary data\n");

        using var context = new LibraryContext(useLazyLoading: false);

        // Single level Include
        Console.WriteLine("1. Single level Include - Authors with Books:");
        var authorsWithBooks = context.Authors
            .Include(a => a.Books)
            .ToList();

        foreach (var author in authorsWithBooks)
        {
            Console.WriteLine($"\n  {author.FirstName} {author.LastName}");
            Console.WriteLine($"  Books: {author.Books.Count}");
            foreach (var book in author.Books)
            {
                Console.WriteLine($"    - {book.Title}");
            }
        }

        // Multiple Includes
        Console.WriteLine("\n\n2. Multiple Includes - Authors with Books and Profile:");
        var authorsComplete = context.Authors
            .Include(a => a.Books)
            .Include(a => a.Profile)
            .ToList();

        foreach (var author in authorsComplete.Take(1))
        {
            Console.WriteLine($"\n  {author.FirstName} {author.LastName}");
            Console.WriteLine($"  Books: {author.Books.Count}");
            if (author.Profile != null)
            {
                Console.WriteLine($"  Website: {author.Profile.Website}");
                Console.WriteLine($"  Experience: {author.Profile.YearsExperience} years");
            }
        }

        // ThenInclude for nested relationships
        Console.WriteLine("\n\n3. ThenInclude - Books with Author and Reviews:");
        var booksWithDetails = context.Books
            .Include(b => b.Author)
            .Include(b => b.Reviews)
            .Include(b => b.Categories)
            .ToList();

        foreach (var book in booksWithDetails.Take(2))
        {
            Console.WriteLine($"\n  Book: {book.Title}");
            Console.WriteLine($"  Author: {book.Author.FirstName} {book.Author.LastName}");
            Console.WriteLine($"  Reviews: {book.Reviews.Count}");
            Console.WriteLine($"  Categories: {string.Join(", ", book.Categories.Select(c => c.Name))}");
        }

        // Filtered Include (EF Core 5+)
        Console.WriteLine("\n\n4. Filtered Include - Books with highly-rated reviews only:");
        var booksWithGoodReviews = context.Books
            .Include(b => b.Reviews.Where(r => r.Rating >= 4))
            .ToList();

        foreach (var book in booksWithGoodReviews.Where(b => b.Reviews.Any()))
        {
            Console.WriteLine($"\n  {book.Title}");
            foreach (var review in book.Reviews)
            {
                Console.WriteLine($"    ⭐ {review.Rating}/5 - {review.ReviewerName}");
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates Explicit Loading
    /// Manually load related data when needed
    /// </summary>
    static void DemonstrateExplicitLoading()
    {
        Console.WriteLine("\n--- EXPLICIT LOADING ---\n");
        Console.WriteLine("Manually loads related data on demand");
        Console.WriteLine("Pros: Full control, load only what's needed");
        Console.WriteLine("Cons: Requires manual loading, multiple queries\n");

        using var context = new LibraryContext(useLazyLoading: false);

        // Load entity first
        var author = context.Authors.First();
        Console.WriteLine($"1. Loaded author: {author.FirstName} {author.LastName}");
        Console.WriteLine($"   Books loaded: {author.Books.Count}"); // 0

        // Explicitly load collection
        Console.WriteLine("\n2. Explicitly loading Books collection...");
        context.Entry(author).Collection(a => a.Books).Load();
        Console.WriteLine($"   Books loaded: {author.Books.Count}");

        foreach (var book in author.Books.Take(2))
        {
            Console.WriteLine($"     - {book.Title}");
        }

        // Explicitly load reference
        Console.WriteLine("\n3. Explicitly loading Profile reference...");
        context.Entry(author).Reference(a => a.Profile).Load();
        if (author.Profile != null)
        {
            Console.WriteLine($"   Website: {author.Profile.Website}");
        }

        // Query related data with filtering
        Console.WriteLine("\n4. Query related data with filtering:");
        var recentBooksCount = context.Entry(author)
            .Collection(a => a.Books)
            .Query()
            .Where(b => b.PublishedDate.Year >= 2010)
            .Count();
        Console.WriteLine($"   Books published since 2010: {recentBooksCount}");

        // Load with additional filtering
        var book = context.Books.First();
        Console.WriteLine($"\n5. Book: {book.Title}");
        Console.WriteLine($"   Loading reviews with rating >= 4...");

        context.Entry(book)
            .Collection(b => b.Reviews)
            .Query()
            .Where(r => r.Rating >= 4)
            .Load();

        Console.WriteLine($"   Highly-rated reviews loaded: {book.Reviews.Count}");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates Lazy Loading
    /// Automatically loads related data when accessed
    /// </summary>
    static void DemonstrateLazyLoading()
    {
        Console.WriteLine("\n--- LAZY LOADING ---\n");
        Console.WriteLine("Automatically loads related data when accessed");
        Console.WriteLine("Pros: Convenient, loads data as needed");
        Console.WriteLine("Cons: N+1 query problem, requires virtual properties\n");

        // NOTE: Requires UseLazyLoadingProxies() and virtual navigation properties
        using var context = new LibraryContext(useLazyLoading: true);

        Console.WriteLine("1. Loading author (without Include):");
        var author = context.Authors.First();
        Console.WriteLine($"   Author loaded: {author.FirstName} {author.LastName}");

        Console.WriteLine("\n2. Accessing Books property (triggers lazy load):");
        Console.WriteLine($"   Books count: {author.Books.Count}");
        // EF Core automatically loaded Books when we accessed the property

        Console.WriteLine("   Book titles:");
        foreach (var book in author.Books.Take(2))
        {
            Console.WriteLine($"     - {book.Title}");

            // Accessing Reviews triggers another lazy load
            Console.WriteLine($"       Reviews: {book.Reviews.Count}");
        }

        Console.WriteLine("\n3. Accessing Profile (triggers lazy load):");
        if (author.Profile != null)
        {
            Console.WriteLine($"   Website: {author.Profile.Website}");
        }

        Console.WriteLine("\nNOTE: Lazy loading generated multiple database queries!");
        Console.WriteLine("This is the N+1 query problem - use with caution.");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates Select/Projection Loading
    /// Load only specific properties needed
    /// </summary>
    static void DemonstrateSelectLoading()
    {
        Console.WriteLine("\n--- SELECT LOADING (Projection) ---\n");
        Console.WriteLine("Load only the data you need using Select()");
        Console.WriteLine("Pros: Most efficient, minimal data transfer");
        Console.WriteLine("Cons: Returns anonymous types or DTOs, not entities\n");

        using var context = new LibraryContext(useLazyLoading: false);

        // Project to anonymous type
        Console.WriteLine("1. Project to anonymous type:");
        var authorSummaries = context.Authors
            .Select(a => new
            {
                FullName = $"{a.FirstName} {a.LastName}",
                Email = a.Email,
                BookCount = a.Books.Count,
                BookTitles = a.Books.Select(b => b.Title).ToList()
            })
            .ToList();

        foreach (var summary in authorSummaries.Take(2))
        {
            Console.WriteLine($"\n  {summary.FullName} ({summary.Email})");
            Console.WriteLine($"  Books written: {summary.BookCount}");
            foreach (var title in summary.BookTitles)
            {
                Console.WriteLine($"    - {title}");
            }
        }

        // Project with nested data
        Console.WriteLine("\n\n2. Project with calculated fields:");
        var bookStats = context.Books
            .Select(b => new
            {
                b.Title,
                Author = $"{b.Author.FirstName} {b.Author.LastName}",
                ReviewCount = b.Reviews.Count,
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                Categories = string.Join(", ", b.Categories.Select(c => c.Name))
            })
            .ToList();

        foreach (var stat in bookStats.Take(3))
        {
            Console.WriteLine($"\n  📖 {stat.Title}");
            Console.WriteLine($"     Author: {stat.Author}");
            Console.WriteLine($"     Reviews: {stat.ReviewCount}");
            Console.WriteLine($"     Avg Rating: {stat.AverageRating:F1}⭐");
            Console.WriteLine($"     Categories: {stat.Categories}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Compares performance of different loading strategies
    /// </summary>
    static void PerformanceComparison()
    {
        Console.WriteLine("\n--- PERFORMANCE COMPARISON ---\n");

        const int iterations = 100;

        // Eager Loading
        var eagerWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var context = new LibraryContext(useLazyLoading: false);
            var authors = context.Authors
                .Include(a => a.Books)
                .Include(a => a.Profile)
                .ToList();
            var bookCount = authors.Sum(a => a.Books.Count);
        }
        eagerWatch.Stop();

        // Explicit Loading
        var explicitWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var context = new LibraryContext(useLazyLoading: false);
            var authors = context.Authors.ToList();
            foreach (var author in authors)
            {
                context.Entry(author).Collection(a => a.Books).Load();
                context.Entry(author).Reference(a => a.Profile).Load();
            }
            var bookCount = authors.Sum(a => a.Books.Count);
        }
        explicitWatch.Stop();

        // Lazy Loading
        var lazyWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var context = new LibraryContext(useLazyLoading: true);
            var authors = context.Authors.ToList();
            var bookCount = authors.Sum(a => a.Books.Count); // Triggers lazy loading
        }
        lazyWatch.Stop();

        // Select/Projection
        var selectWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var context = new LibraryContext(useLazyLoading: false);
            var summaries = context.Authors
                .Select(a => new
                {
                    a.FirstName,
                    a.LastName,
                    BookCount = a.Books.Count
                })
                .ToList();
            var bookCount = summaries.Sum(s => s.BookCount);
        }
        selectWatch.Stop();

        Console.WriteLine($"Eager Loading:      {eagerWatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Explicit Loading:   {explicitWatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Lazy Loading:       {lazyWatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Select/Projection:  {selectWatch.ElapsedMilliseconds}ms");

        Console.WriteLine("\n📊 Analysis:");
        Console.WriteLine("• Eager Loading: Good for loading related data upfront");
        Console.WriteLine("• Explicit Loading: Flexible but requires manual work");
        Console.WriteLine("• Lazy Loading: Convenient but can cause N+1 problems");
        Console.WriteLine("• Select/Projection: Most efficient for read-only data");
    }
}
