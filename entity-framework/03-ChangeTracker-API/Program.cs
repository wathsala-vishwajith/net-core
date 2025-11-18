using ChangeTrackerAPI.Data;
using ChangeTrackerAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ChangeTrackerAPI;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Change Tracker API Demo ===\n");

        // Initialize database
        using (var context = new BlogContext())
        {
            context.Database.EnsureCreated();
        }

        // Demonstrate Change Tracker fundamentals
        DemonstrateEntityStates();

        // Demonstrate detecting changes
        DemonstrateChangeDetection();

        // Demonstrate tracking changes
        DemonstrateTrackingChanges();

        // Demonstrate accessing original and current values
        DemonstrateValueAccess();

        // Demonstrate no-tracking queries
        DemonstrateNoTracking();

        Console.WriteLine("\n=== Demo Complete ===");
    }

    /// <summary>
    /// Demonstrates different entity states in EF Core
    /// </summary>
    static void DemonstrateEntityStates()
    {
        Console.WriteLine("--- Entity States Demo ---\n");

        using var context = new BlogContext();

        // STATE: Detached
        var newPost = new BlogPost
        {
            Title = "Understanding Entity States",
            Content = "This post explains entity states...",
            Author = "State Master",
            PublishedDate = DateTime.Now,
            IsPublished = false
        };

        var entry = context.Entry(newPost);
        Console.WriteLine($"1. New entity state: {entry.State}"); // Detached

        // STATE: Added
        context.BlogPosts.Add(newPost);
        Console.WriteLine($"2. After Add() state: {entry.State}"); // Added

        // STATE: Unchanged
        context.SaveChanges();
        Console.WriteLine($"3. After SaveChanges() state: {entry.State}"); // Unchanged

        // STATE: Modified
        newPost.ViewCount = 10;
        Console.WriteLine($"4. After property change state: {entry.State}"); // Modified

        context.SaveChanges();
        Console.WriteLine($"5. After SaveChanges() state: {entry.State}"); // Unchanged

        // STATE: Deleted
        context.BlogPosts.Remove(newPost);
        Console.WriteLine($"6. After Remove() state: {entry.State}"); // Deleted

        context.SaveChanges();
        Console.WriteLine($"7. After SaveChanges() state: {entry.State}\n"); // Detached
    }

    /// <summary>
    /// Demonstrates how EF Core detects changes
    /// </summary>
    static void DemonstrateChangeDetection()
    {
        Console.WriteLine("--- Change Detection Demo ---\n");

        using var context = new BlogContext();

        var post = context.BlogPosts.First();
        Console.WriteLine($"Original title: {post.Title}");
        Console.WriteLine($"Original view count: {post.ViewCount}");

        // Make changes
        post.Title = "Updated Title";
        post.ViewCount += 1;
        post.LastModifiedDate = DateTime.Now;

        // Manually trigger change detection
        context.ChangeTracker.DetectChanges();

        Console.WriteLine("\nAfter DetectChanges():");

        // Get all tracked entities
        var entries = context.ChangeTracker.Entries();
        foreach (var entry in entries)
        {
            Console.WriteLine($"\nEntity: {entry.Entity.GetType().Name}");
            Console.WriteLine($"State: {entry.State}");

            if (entry.State == EntityState.Modified)
            {
                Console.WriteLine("Modified properties:");
                foreach (var property in entry.Properties)
                {
                    if (property.IsModified)
                    {
                        Console.WriteLine($"  - {property.Metadata.Name}: " +
                            $"{property.OriginalValue} → {property.CurrentValue}");
                    }
                }
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates tracking entity changes
    /// </summary>
    static void DemonstrateTrackingChanges()
    {
        Console.WriteLine("--- Tracking Changes Demo ---\n");

        using var context = new BlogContext();

        // Enable auto-detect changes (default behavior)
        context.ChangeTracker.AutoDetectChangesEnabled = true;

        var post = context.BlogPosts
            .Include(p => p.Comments)
            .First();

        Console.WriteLine($"Blog Post: {post.Title}");
        Console.WriteLine($"Comments count: {post.Comments.Count}");

        // Add a comment
        var newComment = new Comment
        {
            Text = "This is a tracked comment",
            AuthorName = "Tracker User",
            CreatedDate = DateTime.Now,
            BlogPostId = post.BlogPostId
        };

        post.Comments.Add(newComment);

        // Check what's being tracked
        Console.WriteLine("\nTracked entities before SaveChanges:");
        foreach (var entry in context.ChangeTracker.Entries())
        {
            Console.WriteLine($"  - {entry.Entity.GetType().Name}: {entry.State}");
        }

        context.SaveChanges();

        Console.WriteLine("\nTracked entities after SaveChanges:");
        foreach (var entry in context.ChangeTracker.Entries())
        {
            Console.WriteLine($"  - {entry.Entity.GetType().Name}: {entry.State}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates accessing original and current values
    /// </summary>
    static void DemonstrateValueAccess()
    {
        Console.WriteLine("--- Original vs Current Values Demo ---\n");

        using var context = new BlogContext();

        var post = context.BlogPosts.First();

        Console.WriteLine("Original values:");
        Console.WriteLine($"  Title: {post.Title}");
        Console.WriteLine($"  ViewCount: {post.ViewCount}");
        Console.WriteLine($"  IsPublished: {post.IsPublished}");

        // Make multiple changes
        post.Title = "Completely New Title";
        post.ViewCount = 500;
        post.IsPublished = false;

        var entry = context.Entry(post);

        Console.WriteLine("\nAfter modifications:");

        foreach (var property in entry.Properties)
        {
            if (property.IsModified)
            {
                Console.WriteLine($"\n  Property: {property.Metadata.Name}");
                Console.WriteLine($"    Original: {property.OriginalValue}");
                Console.WriteLine($"    Current: {property.CurrentValue}");
                Console.WriteLine($"    Is Modified: {property.IsModified}");
            }
        }

        // Reset a specific property to original value
        var titleProperty = entry.Property(p => p.Title);
        titleProperty.CurrentValue = (string)titleProperty.OriginalValue!;
        titleProperty.IsModified = false;

        Console.WriteLine("\nAfter resetting Title:");
        Console.WriteLine($"  Title: {post.Title}");
        Console.WriteLine($"  Title IsModified: {titleProperty.IsModified}");

        // Check which properties are still modified
        Console.WriteLine("\nStill modified:");
        foreach (var property in entry.Properties.Where(p => p.IsModified))
        {
            Console.WriteLine($"  - {property.Metadata.Name}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates no-tracking queries for read-only scenarios
    /// </summary>
    static void DemonstrateNoTracking()
    {
        Console.WriteLine("--- No-Tracking Queries Demo ---\n");

        // Tracking query (default)
        using (var context = new BlogContext())
        {
            Console.WriteLine("Tracking Query:");
            var posts = context.BlogPosts.ToList();

            Console.WriteLine($"  Retrieved {posts.Count} posts");
            Console.WriteLine($"  Tracked entities: {context.ChangeTracker.Entries().Count()}");

            // Modify a post
            posts[0].ViewCount += 1;

            // Check if change is detected
            var modifiedCount = context.ChangeTracker.Entries()
                .Count(e => e.State == EntityState.Modified);
            Console.WriteLine($"  Modified entities: {modifiedCount}");
        }

        Console.WriteLine();

        // No-tracking query (better for read-only)
        using (var context = new BlogContext())
        {
            Console.WriteLine("No-Tracking Query:");
            var posts = context.BlogPosts
                .AsNoTracking()
                .ToList();

            Console.WriteLine($"  Retrieved {posts.Count} posts");
            Console.WriteLine($"  Tracked entities: {context.ChangeTracker.Entries().Count()}");

            // Modify a post (won't be tracked)
            posts[0].ViewCount += 1;

            var modifiedCount = context.ChangeTracker.Entries()
                .Count(e => e.State == EntityState.Modified);
            Console.WriteLine($"  Modified entities: {modifiedCount}");
        }

        Console.WriteLine();

        // Global no-tracking configuration
        using (var context = new BlogContext())
        {
            Console.WriteLine("Global No-Tracking:");
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var posts = context.BlogPosts.ToList();
            Console.WriteLine($"  Retrieved {posts.Count} posts");
            Console.WriteLine($"  Tracked entities: {context.ChangeTracker.Entries().Count()}");
        }

        Console.WriteLine();

        // Performance comparison
        PerformanceComparison();
    }

    /// <summary>
    /// Compares performance of tracking vs no-tracking queries
    /// </summary>
    static void PerformanceComparison()
    {
        Console.WriteLine("--- Performance Comparison ---\n");

        const int iterations = 1000;

        // Tracking queries
        var trackingWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var context = new BlogContext();
            var posts = context.BlogPosts.ToList();
        }
        trackingWatch.Stop();

        // No-tracking queries
        var noTrackingWatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var context = new BlogContext();
            var posts = context.BlogPosts.AsNoTracking().ToList();
        }
        noTrackingWatch.Stop();

        Console.WriteLine($"Tracking queries ({iterations} iterations): {trackingWatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"No-tracking queries ({iterations} iterations): {noTrackingWatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Performance gain: {trackingWatch.ElapsedMilliseconds - noTrackingWatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Percentage improvement: {((trackingWatch.ElapsedMilliseconds - noTrackingWatch.ElapsedMilliseconds) / (double)trackingWatch.ElapsedMilliseconds * 100):F2}%");
    }
}
