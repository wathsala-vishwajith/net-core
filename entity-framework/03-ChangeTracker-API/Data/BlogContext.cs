using Microsoft.EntityFrameworkCore;
using ChangeTrackerAPI.Models;

namespace ChangeTrackerAPI.Data;

/// <summary>
/// DbContext for Blog database
/// </summary>
public class BlogContext : DbContext
{
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=blog.db");
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure BlogPost
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.BlogPostId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Author).IsRequired().HasMaxLength(100);
        });

        // Configure Comment
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.AuthorName).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.BlogPost)
                .WithMany(b => b.Comments)
                .HasForeignKey(e => e.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();

            // Many-to-many relationship
            entity.HasMany(t => t.BlogPosts)
                .WithMany(b => b.Tags);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogPost>().HasData(
            new BlogPost
            {
                BlogPostId = 1,
                Title = "Getting Started with EF Core",
                Content = "Entity Framework Core is a modern ORM...",
                Author = "John Developer",
                PublishedDate = DateTime.Now.AddMonths(-2),
                ViewCount = 150,
                IsPublished = true
            },
            new BlogPost
            {
                BlogPostId = 2,
                Title = "Advanced Change Tracking",
                Content = "Understanding how EF Core tracks changes...",
                Author = "Jane Architect",
                PublishedDate = DateTime.Now.AddMonths(-1),
                ViewCount = 89,
                IsPublished = true
            }
        );

        modelBuilder.Entity<Tag>().HasData(
            new Tag { TagId = 1, Name = "EF Core" },
            new Tag { TagId = 2, Name = "C#" },
            new Tag { TagId = 3, Name = "Database" }
        );

        modelBuilder.Entity<Comment>().HasData(
            new Comment
            {
                CommentId = 1,
                Text = "Great article!",
                AuthorName = "Reader1",
                CreatedDate = DateTime.Now.AddMonths(-2).AddDays(1),
                BlogPostId = 1
            },
            new Comment
            {
                CommentId = 2,
                Text = "Very informative, thanks!",
                AuthorName = "Reader2",
                CreatedDate = DateTime.Now.AddMonths(-2).AddDays(2),
                BlogPostId = 1
            }
        );
    }
}
