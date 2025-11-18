using Microsoft.EntityFrameworkCore;
using LoadingStrategies.Models;

namespace LoadingStrategies.Data;

/// <summary>
/// DbContext for Library database
/// Configured to support different loading strategies
/// </summary>
public class LibraryContext : DbContext
{
    private readonly bool _useLazyLoading;

    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<AuthorProfile> AuthorProfiles { get; set; }

    public LibraryContext(bool useLazyLoading = false)
    {
        _useLazyLoading = useLazyLoading;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=library.db");

        // Enable lazy loading if requested
        if (_useLazyLoading)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Author
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.AuthorId);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure Book
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ISBN).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.ISBN).IsUnique();

            entity.HasOne(e => e.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Review
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId);
            entity.Property(e => e.ReviewerName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(1000);

            entity.HasOne(e => e.Book)
                .WithMany(b => b.Reviews)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();

            // Many-to-many relationship with Book
            entity.HasMany(c => c.Books)
                .WithMany(b => b.Categories);
        });

        // Configure AuthorProfile (one-to-one)
        modelBuilder.Entity<AuthorProfile>(entity =>
        {
            entity.HasKey(e => e.AuthorProfileId);

            entity.HasOne(e => e.Author)
                .WithOne(a => a.Profile)
                .HasForeignKey<AuthorProfile>(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Authors
        modelBuilder.Entity<Author>().HasData(
            new Author { AuthorId = 1, FirstName = "Robert", LastName = "Martin", Email = "uncle.bob@cleancode.com", Bio = "Software craftsman and author" },
            new Author { AuthorId = 2, FirstName = "Martin", LastName = "Fowler", Email = "martin@refactoring.com", Bio = "Chief Scientist at ThoughtWorks" },
            new Author { AuthorId = 3, FirstName = "Eric", LastName = "Evans", Email = "eric@domaindrivendesign.org", Bio = "Domain-Driven Design pioneer" }
        );

        // Author Profiles
        modelBuilder.Entity<AuthorProfile>().HasData(
            new AuthorProfile { AuthorProfileId = 1, AuthorId = 1, Website = "cleancoder.com", Twitter = "@unclebobmartin", LinkedIn = "robertcmartin", YearsExperience = 50 },
            new AuthorProfile { AuthorProfileId = 2, AuthorId = 2, Website = "martinfowler.com", Twitter = "@martinfowler", LinkedIn = "martinfowler", YearsExperience = 40 },
            new AuthorProfile { AuthorProfileId = 3, AuthorId = 3, Website = "domainlanguage.com", Twitter = "@ericevans0", LinkedIn = "ericevansddd", YearsExperience = 35 }
        );

        // Books
        modelBuilder.Entity<Book>().HasData(
            new Book { BookId = 1, Title = "Clean Code", ISBN = "978-0132350884", PublishedDate = new DateTime(2008, 8, 1), Pages = 464, AuthorId = 1 },
            new Book { BookId = 2, Title = "Clean Architecture", ISBN = "978-0134494166", PublishedDate = new DateTime(2017, 9, 20), Pages = 432, AuthorId = 1 },
            new Book { BookId = 3, Title = "Refactoring", ISBN = "978-0134757599", PublishedDate = new DateTime(2018, 11, 20), Pages = 448, AuthorId = 2 },
            new Book { BookId = 4, Title = "Patterns of Enterprise Application Architecture", ISBN = "978-0321127426", PublishedDate = new DateTime(2002, 11, 15), Pages = 560, AuthorId = 2 },
            new Book { BookId = 5, Title = "Domain-Driven Design", ISBN = "978-0321125217", PublishedDate = new DateTime(2003, 8, 30), Pages = 560, AuthorId = 3 }
        );

        // Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Software Engineering", Description = "Books about software engineering practices" },
            new Category { CategoryId = 2, Name = "Design Patterns", Description = "Books about software design patterns" },
            new Category { CategoryId = 3, Name = "Architecture", Description = "Books about software architecture" },
            new Category { CategoryId = 4, Name = "Best Practices", Description = "Books about software development best practices" }
        );

        // Reviews
        modelBuilder.Entity<Review>().HasData(
            new Review { ReviewId = 1, BookId = 1, ReviewerName = "John Developer", Rating = 5, Comment = "Essential reading for any programmer!", ReviewDate = DateTime.Now.AddMonths(-6) },
            new Review { ReviewId = 2, BookId = 1, ReviewerName = "Jane Coder", Rating = 5, Comment = "Changed the way I write code.", ReviewDate = DateTime.Now.AddMonths(-5) },
            new Review { ReviewId = 3, BookId = 3, ReviewerName = "Mike Architect", Rating = 5, Comment = "Great insights into refactoring techniques.", ReviewDate = DateTime.Now.AddMonths(-3) },
            new Review { ReviewId = 4, BookId = 5, ReviewerName = "Sarah Designer", Rating = 4, Comment = "Comprehensive guide to DDD.", ReviewDate = DateTime.Now.AddMonths(-2) }
        );
    }
}
