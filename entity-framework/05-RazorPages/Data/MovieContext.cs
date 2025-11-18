using Microsoft.EntityFrameworkCore;
using RazorPagesApp.Models;

namespace RazorPagesApp.Data;

public class MovieContext : DbContext
{
    public MovieContext(DbContextOptions<MovieContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed initial data
        modelBuilder.Entity<Movie>().HasData(
            new Movie
            {
                Id = 1,
                Title = "The Shawshank Redemption",
                ReleaseDate = new DateTime(1994, 9, 23),
                Genre = "Drama",
                Price = 9.99m,
                Description = "Two imprisoned men bond over a number of years.",
                Rating = 9
            },
            new Movie
            {
                Id = 2,
                Title = "The Godfather",
                ReleaseDate = new DateTime(1972, 3, 24),
                Genre = "Crime",
                Price = 12.99m,
                Description = "The aging patriarch of an organized crime dynasty transfers control.",
                Rating = 9
            },
            new Movie
            {
                Id = 3,
                Title = "The Dark Knight",
                ReleaseDate = new DateTime(2008, 7, 18),
                Genre = "Action",
                Price = 14.99m,
                Description = "When the menace known as the Joker wreaks havoc on Gotham.",
                Rating = 9
            },
            new Movie
            {
                Id = 4,
                Title = "Inception",
                ReleaseDate = new DateTime(2010, 7, 16),
                Genre = "Sci-Fi",
                Price = 13.99m,
                Description = "A thief who steals corporate secrets through dream-sharing technology.",
                Rating = 8
            },
            new Movie
            {
                Id = 5,
                Title = "Pulp Fiction",
                ReleaseDate = new DateTime(1994, 10, 14),
                Genre = "Crime",
                Price = 11.99m,
                Description = "The lives of two mob hitmen, a boxer, and others intertwine.",
                Rating = 8
            }
        );
    }
}
