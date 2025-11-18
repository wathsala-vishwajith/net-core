using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesApp.Data;
using RazorPagesApp.Models;

namespace RazorPagesApp.Pages.Movies;

public class IndexModel : PageModel
{
    private readonly MovieContext _context;

    public IndexModel(MovieContext context)
    {
        _context = context;
    }

    public IList<Movie> Movies { get; set; } = new List<Movie>();
    public string? SearchString { get; set; }
    public string? MovieGenre { get; set; }
    public List<string> Genres { get; set; } = new List<string>();

    public async Task OnGetAsync(string? searchString, string? movieGenre)
    {
        // Store search parameters
        SearchString = searchString;
        MovieGenre = movieGenre;

        // Get all genres for dropdown
        Genres = await _context.Movies
            .Select(m => m.Genre)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();

        // Build query
        var moviesQuery = _context.Movies.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(searchString))
        {
            moviesQuery = moviesQuery.Where(m => m.Title.Contains(searchString));
        }

        // Apply genre filter
        if (!string.IsNullOrEmpty(movieGenre))
        {
            moviesQuery = moviesQuery.Where(m => m.Genre == movieGenre);
        }

        // Execute query
        Movies = await moviesQuery
            .OrderBy(m => m.Title)
            .ToListAsync();
    }
}
