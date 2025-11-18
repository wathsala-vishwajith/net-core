# Razor Pages with Entity Framework Core

This project demonstrates how to build a complete web application using **ASP.NET Core Razor Pages** with **Entity Framework Core** for data access.

## Overview

Razor Pages is a page-based programming model that makes building web UI easier and more productive. Combined with EF Core, it provides a complete solution for data-driven web applications.

## Project Structure

```
05-RazorPages/
├── Data/
│   └── MovieContext.cs              # EF Core DbContext
├── Models/
│   └── Movie.cs                     # Movie entity with validation
├── Pages/
│   ├── Movies/
│   │   ├── Index.cshtml/.cs         # List movies with search/filter
│   │   ├── Create.cshtml/.cs        # Create new movie
│   │   ├── Edit.cshtml/.cs          # Edit existing movie
│   │   ├── Delete.cshtml/.cs        # Delete movie
│   │   └── Details.cshtml/.cs       # View movie details
│   ├── Shared/
│   │   ├── _Layout.cshtml           # Master layout
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── Index.cshtml/.cs             # Home page
│   ├── _ViewImports.cshtml          # Global imports
│   └── _ViewStart.cshtml            # Layout configuration
├── wwwroot/
│   ├── css/site.css                 # Styles
│   └── js/site.js                   # JavaScript
├── Program.cs                       # Application entry point
├── appsettings.json                 # Configuration
├── RazorPagesApp.csproj            # Project file
└── README.md                        # This file
```

## Key Concepts

### 1. Razor Pages Architecture

**PageModel Pattern (Code-Behind)**

Each Razor Page consists of two files:
- `.cshtml` - View markup (HTML + Razor syntax)
- `.cshtml.cs` - PageModel (code-behind with handlers)

```csharp
// Index.cshtml.cs
public class IndexModel : PageModel
{
    private readonly MovieContext _context;

    public IndexModel(MovieContext context)
    {
        _context = context;
    }

    public IList<Movie> Movies { get; set; }

    public async Task OnGetAsync()
    {
        Movies = await _context.Movies.ToListAsync();
    }
}
```

**Location:** `Pages/Movies/Index.cshtml.cs:1-51`

### 2. Page Handlers

Razor Pages use handler methods to respond to HTTP requests:

| Handler | HTTP Verb | Purpose |
|---------|-----------|---------|
| `OnGet()` | GET | Display page/data |
| `OnPost()` | POST | Process form submission |
| `OnGetEdit()` | GET | Named handler for specific action |
| `OnPostDelete()` | POST | Named handler for delete |

```csharp
// GET handler
public async Task<IActionResult> OnGetAsync(int? id)
{
    Movie = await _context.Movies.FindAsync(id);
    return Page();
}

// POST handler
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
    {
        return Page();
    }
    _context.Movies.Add(Movie);
    await _context.SaveChangesAsync();
    return RedirectToPage("./Index");
}
```

**Location:** `Pages/Movies/Create.cshtml.cs`

### 3. Model Binding

The `[BindProperty]` attribute enables automatic binding of form data to properties:

```csharp
public class CreateModel : PageModel
{
    [BindProperty]
    public Movie Movie { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        // Movie is automatically populated from form
        _context.Movies.Add(Movie);
        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
}
```

**Location:** `Pages/Movies/Create.cshtml.cs:14`

### 4. Data Annotations

Validation attributes on the model:

```csharp
public class Movie
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    [Display(Name = "Release Date")]
    [DataType(DataType.Date)]
    public DateTime ReleaseDate { get; set; }

    [Range(0.01, 1000.00)]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Range(1, 10)]
    public int Rating { get; set; }
}
```

**Location:** `Models/Movie.cs`

### 5. Razor Syntax

Mixing C# code with HTML:

```html
<!-- Display property -->
<h1>@Model.Movie.Title</h1>

<!-- Loop through collection -->
@foreach (var movie in Model.Movies)
{
    <tr>
        <td>@movie.Title</td>
        <td>@movie.Price.ToString("C")</td>
    </tr>
}

<!-- Conditional rendering -->
@if (!Model.Movies.Any())
{
    <div class="alert alert-info">No movies found.</div>
}

<!-- For loop for ratings -->
@for (int i = 0; i < movie.Rating; i++)
{
    <span>⭐</span>
}
```

**Location:** `Pages/Movies/Index.cshtml`

### 6. Tag Helpers

Server-side code that participates in creating HTML elements:

```html
<!-- Form tag helper -->
<form method="post">
    <!-- Input tag helper -->
    <input asp-for="Movie.Title" class="form-control" />

    <!-- Validation tag helper -->
    <span asp-validation-for="Movie.Title" class="text-danger"></span>

    <!-- Anchor tag helper (generates route) -->
    <a asp-page="./Edit" asp-route-id="@movie.Id" class="btn btn-warning">Edit</a>
</form>
```

**Location:** `Pages/Movies/Create.cshtml`

### 7. Routing

Razor Pages uses convention-based routing:

| File Path | Route | Example |
|-----------|-------|---------|
| `/Pages/Index.cshtml` | `/` | Home page |
| `/Pages/Movies/Index.cshtml` | `/Movies` | Movie list |
| `/Pages/Movies/Create.cshtml` | `/Movies/Create` | Create form |
| `/Pages/Movies/Edit.cshtml` | `/Movies/Edit/{id}` | Edit page |

**Route Parameters:**

```csharp
// Pages/Movies/Details.cshtml
@page "{id:int}"

// Pages/Movies/Edit.cshtml.cs
public async Task<IActionResult> OnGetAsync(int? id)
{
    // id comes from route
}
```

**Location:** `Pages/Movies/Details.cshtml:1`

### 8. Dependency Injection

DbContext is injected via constructor:

```csharp
public class IndexModel : PageModel
{
    private readonly MovieContext _context;

    public IndexModel(MovieContext context)
    {
        _context = context;
    }

    // Use _context in handlers
}
```

**Location:** `Pages/Movies/Index.cshtml.cs:9-13`

## CRUD Operations

### Create (Insert)

```csharp
// Create.cshtml.cs
[BindProperty]
public Movie Movie { get; set; }

public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
        return Page();

    _context.Movies.Add(Movie);
    await _context.SaveChangesAsync();
    return RedirectToPage("./Index");
}
```

### Read (Query)

```csharp
// Index.cshtml.cs
public async Task OnGetAsync(string searchString, string movieGenre)
{
    var moviesQuery = _context.Movies.AsQueryable();

    if (!string.IsNullOrEmpty(searchString))
    {
        moviesQuery = moviesQuery.Where(m => m.Title.Contains(searchString));
    }

    Movies = await moviesQuery.ToListAsync();
}
```

### Update

```csharp
// Edit.cshtml.cs
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
        return Page();

    _context.Attach(Movie).State = EntityState.Modified;

    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!MovieExists(Movie.Id))
            return NotFound();
        throw;
    }

    return RedirectToPage("./Index");
}
```

### Delete

```csharp
// Delete.cshtml.cs
public async Task<IActionResult> OnPostAsync()
{
    var movie = await _context.Movies.FindAsync(Movie.Id);

    if (movie != null)
    {
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
    }

    return RedirectToPage("./Index");
}
```

## Features Demonstrated

### 1. Search and Filter

```csharp
// Query building
var moviesQuery = _context.Movies.AsQueryable();

// Search by title
if (!string.IsNullOrEmpty(searchString))
{
    moviesQuery = moviesQuery.Where(m => m.Title.Contains(searchString));
}

// Filter by genre
if (!string.IsNullOrEmpty(movieGenre))
{
    moviesQuery = moviesQuery.Where(m => m.Genre == movieGenre);
}

Movies = await moviesQuery.OrderBy(m => m.Title).ToListAsync();
```

**Location:** `Pages/Movies/Index.cshtml.cs:24-46`

### 2. Validation

**Server-side:**
```csharp
if (!ModelState.IsValid)
{
    return Page(); // Redisplay form with errors
}
```

**Client-side:**
```html
@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

**Location:** `Pages/Movies/Create.cshtml:64-66`

### 3. Layout and Shared Views

```html
<!-- _Layout.cshtml -->
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"]</title>
</head>
<body>
    <nav>...</nav>
    @RenderBody()
    <footer>...</footer>
</body>
</html>
```

**Location:** `Pages/Shared/_Layout.cshtml`

## Configuration

### DbContext Registration

```csharp
// Program.cs
builder.Services.AddDbContext<MovieContext>(options =>
    options.UseSqlite("Data Source=movies.db"));
```

**Location:** `Program.cs:9-10`

### Database Initialization

```csharp
// Ensure database is created on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MovieContext>();
    context.Database.EnsureCreated();
}
```

**Location:** `Program.cs:17-22`

## Running the Application

### 1. Restore Packages

```bash
dotnet restore
```

### 2. Run the Application

```bash
dotnet run
```

Or:

```bash
dotnet watch run  # Auto-reload on file changes
```

### 3. Access the Application

Open browser to: `https://localhost:5001` (or port shown in console)

### 4. Navigate

- **Home:** `/`
- **Movies List:** `/Movies`
- **Create Movie:** `/Movies/Create`
- **Edit Movie:** `/Movies/Edit/1`
- **Details:** `/Movies/Details/1`
- **Delete:** `/Movies/Delete/1`

## Project Features

| Feature | Implementation |
|---------|----------------|
| **CRUD Operations** | Full Create, Read, Update, Delete |
| **Search** | Filter movies by title |
| **Filter** | Filter by genre dropdown |
| **Validation** | Client and server-side |
| **Routing** | Convention-based and attribute |
| **Dependency Injection** | DbContext injection |
| **Bootstrap UI** | Responsive design |
| **Data Annotations** | Model validation |
| **Tag Helpers** | Form and link generation |

## Best Practices

### 1. Use Async/Await

```csharp
// ✅ Good
public async Task OnGetAsync()
{
    Movies = await _context.Movies.ToListAsync();
}

// ❌ Bad
public void OnGet()
{
    Movies = _context.Movies.ToList();
}
```

### 2. Validate Input

```csharp
if (!ModelState.IsValid)
{
    return Page();
}
```

### 3. Handle Concurrency

```csharp
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException)
{
    if (!MovieExists(Movie.Id))
        return NotFound();
    throw;
}
```

### 4. Use BindProperty Selectively

```csharp
// ✅ POST only
[BindProperty]
public Movie Movie { get; set; }

// Also works for GET and POST
[BindProperty(SupportsGet = true)]
public string SearchString { get; set; }
```

### 5. Redirect After POST

```csharp
// PRG pattern (Post-Redirect-Get)
public async Task<IActionResult> OnPostAsync()
{
    // ... save data
    return RedirectToPage("./Index"); // Redirect, don't return Page()
}
```

## Learn More

- [Razor Pages Documentation](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/)
- [Razor Syntax Reference](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor)
- [Tag Helpers](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/intro)
- [Model Binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding)
- [Validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)

## Related Projects

- **01-EFCore-Basics**: EF Core fundamentals
- **02-CodeFirst-Migrations**: Database migrations
- **06-Middlewares**: ASP.NET Core middleware
- **07-Minimal-APIs**: Minimal API approach
