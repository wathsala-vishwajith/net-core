using System.ComponentModel.DataAnnotations;

namespace RazorPagesApp.Models;

/// <summary>
/// Represents a movie entity
/// </summary>
public class Movie
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Release Date")]
    [DataType(DataType.Date)]
    public DateTime ReleaseDate { get; set; }

    [Required]
    [StringLength(50)]
    public string Genre { get; set; } = string.Empty;

    [Range(0.01, 1000.00)]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(1, 10)]
    public int Rating { get; set; }
}
