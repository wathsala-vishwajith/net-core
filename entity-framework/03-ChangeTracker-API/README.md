# Change Tracker API

This project demonstrates Entity Framework Core's **Change Tracker API**, which is responsible for tracking changes to entities and determining what needs to be saved to the database.

## Overview

The Change Tracker is a core component of EF Core that:
- Tracks entity states (Added, Modified, Deleted, Unchanged, Detached)
- Detects property changes
- Maintains original and current values
- Determines what SQL to generate when SaveChanges() is called
- Provides fine-grained control over entity tracking

## Project Structure

```
03-ChangeTracker-API/
├── Data/
│   └── BlogContext.cs           # DbContext configuration
├── Models/
│   ├── BlogPost.cs              # Blog post entity
│   ├── Comment.cs               # Comment entity
│   └── Tag.cs                   # Tag entity
├── Program.cs                   # Comprehensive demonstrations
├── ChangeTrackerAPI.csproj     # Project file
└── README.md                    # This file
```

## Key Concepts

### 1. Entity States

Every tracked entity is in one of five states:

| State | Description | Next SaveChanges() Action |
|-------|-------------|---------------------------|
| **Detached** | Not tracked by context | No action |
| **Unchanged** | Tracked, no modifications | No action |
| **Added** | New entity, will be inserted | INSERT |
| **Modified** | Tracked with modifications | UPDATE |
| **Deleted** | Marked for deletion | DELETE |

**Location:** `Program.cs:27-56`

#### State Transitions Example

```csharp
var post = new BlogPost { Title = "New Post" };
// State: Detached

context.BlogPosts.Add(post);
// State: Added

context.SaveChanges();
// State: Unchanged (after INSERT)

post.Title = "Updated Title";
// State: Modified

context.BlogPosts.Remove(post);
// State: Deleted

context.SaveChanges();
// State: Detached (after DELETE)
```

### 2. Change Detection

EF Core detects changes in two ways:

#### Automatic Detection
- Triggered automatically by operations like SaveChanges()
- Can be controlled via `AutoDetectChangesEnabled`

```csharp
context.ChangeTracker.AutoDetectChangesEnabled = true; // Default
```

#### Manual Detection
```csharp
post.Title = "New Title";
context.ChangeTracker.DetectChanges(); // Manually trigger
```

**Location:** `Program.cs:58-94`

### 3. Tracking Entities

The ChangeTracker provides access to all tracked entities:

```csharp
// Get all tracked entities
var entries = context.ChangeTracker.Entries();

// Get entities by state
var modified = context.ChangeTracker.Entries()
    .Where(e => e.State == EntityState.Modified);

// Get specific entity type
var posts = context.ChangeTracker.Entries<BlogPost>();
```

**Location:** `Program.cs:96-131`

### 4. Accessing Values

For each tracked entity, you can access:

#### Original Values
Values when entity was first queried from database

```csharp
var entry = context.Entry(post);
var originalTitle = entry.Property(p => p.Title).OriginalValue;
```

#### Current Values
Current in-memory values

```csharp
var currentTitle = entry.Property(p => p.Title).CurrentValue;
```

#### Database Values
Current values in the database (requires query)

```csharp
var databaseValues = await entry.GetDatabaseValuesAsync();
```

**Location:** `Program.cs:133-179`

### 5. No-Tracking Queries

For read-only scenarios, use no-tracking queries for better performance:

```csharp
// Per-query no-tracking
var posts = context.BlogPosts
    .AsNoTracking()
    .ToList();

// Global no-tracking
context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
```

**Benefits:**
- Lower memory usage
- Faster query execution
- No change tracking overhead

**Use when:**
- Read-only data display
- Reporting queries
- DTOs for API responses

**Location:** `Program.cs:181-239`

## Practical Examples

### Example 1: Check What Changed

```csharp
using var context = new BlogContext();
var post = context.BlogPosts.First();

post.Title = "Updated";
post.ViewCount = 100;

var entry = context.Entry(post);
foreach (var property in entry.Properties)
{
    if (property.IsModified)
    {
        Console.WriteLine($"{property.Metadata.Name}: " +
            $"{property.OriginalValue} → {property.CurrentValue}");
    }
}
// Output:
// Title: Original Title → Updated
// ViewCount: 50 → 100
```

### Example 2: Undo Changes

```csharp
var entry = context.Entry(post);

// Reset specific property
entry.Property(p => p.Title).CurrentValue =
    entry.Property(p => p.Title).OriginalValue;
entry.Property(p => p.Title).IsModified = false;

// Or reload all properties from database
entry.Reload();
```

### Example 3: Attach Disconnected Entity

```csharp
// From web API, desktop app, etc.
var updatedPost = new BlogPost
{
    BlogPostId = 1,
    Title = "Updated Title",
    // ... other properties
};

// Attach and mark as modified
context.Attach(updatedPost);
context.Entry(updatedPost).State = EntityState.Modified;
context.SaveChanges(); // UPDATE
```

### Example 4: Partial Updates

```csharp
var post = new BlogPost { BlogPostId = 1 };
context.Attach(post);

// Update only specific properties
var entry = context.Entry(post);
entry.Property(p => p.Title).CurrentValue = "New Title";
entry.Property(p => p.Title).IsModified = true;

context.SaveChanges(); // UPDATE only Title
```

### Example 5: Concurrency Detection

```csharp
var post = context.BlogPosts.Find(1);
post.Title = "Updated";

try
{
    context.SaveChanges();
}
catch (DbUpdateConcurrencyException ex)
{
    var entry = ex.Entries.Single();
    var databaseValues = entry.GetDatabaseValues();

    // Handle concurrency conflict
    Console.WriteLine($"Database value: {databaseValues["Title"]}");
    Console.WriteLine($"Your value: {entry.Property("Title").CurrentValue}");
}
```

## Change Tracker API Reference

### ChangeTracker Properties

```csharp
// Enable/disable automatic change detection
context.ChangeTracker.AutoDetectChangesEnabled = true;

// Set default tracking behavior
context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

// Control cascade delete timing
context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

// Control delete orphans timing
context.ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;
```

### ChangeTracker Methods

```csharp
// Detect all changes
context.ChangeTracker.DetectChanges();

// Get all tracked entities
var entries = context.ChangeTracker.Entries();

// Get tracked entities of specific type
var postEntries = context.ChangeTracker.Entries<BlogPost>();

// Check if any changes
bool hasChanges = context.ChangeTracker.HasChanges();

// Accept all changes (mark everything as Unchanged)
context.ChangeTracker.AcceptAllChanges();

// Clear all tracking
context.ChangeTracker.Clear();
```

### EntityEntry Methods

```csharp
var entry = context.Entry(post);

// Get/set state
entry.State = EntityState.Modified;

// Reload from database
entry.Reload();
await entry.ReloadAsync();

// Get database values
var dbValues = entry.GetDatabaseValues();

// Get reference to related entity
var reference = entry.Reference(p => p.Supplier);

// Get collection of related entities
var collection = entry.Collection(p => p.Comments);

// Access property
var property = entry.Property(p => p.Title);
```

### PropertyEntry Methods

```csharp
var property = entry.Property(p => p.Title);

// Get/set values
var original = property.OriginalValue;
var current = property.CurrentValue;
property.CurrentValue = "New Value";

// Check/set modified flag
bool isModified = property.IsModified;
property.IsModified = true;

// Check if temporary value (for key generation)
bool isTemp = property.IsTemporary;
```

## Performance Considerations

### When to Use Tracking

✅ **Use tracking when:**
- Updating entities
- Need to detect changes
- Working with related entities
- Implementing auditing

### When to Use No-Tracking

✅ **Use no-tracking when:**
- Read-only queries
- Displaying data
- Exporting data
- Building DTOs for APIs

### Performance Comparison

Based on the demo's performance test (1000 iterations):

```
Tracking queries:     ~850ms
No-tracking queries:  ~650ms
Performance gain:     ~200ms (23.5% faster)
```

## Best Practices

### 1. Use No-Tracking for Read-Only Queries
```csharp
// Good for read-only
var posts = context.BlogPosts.AsNoTracking().ToList();
```

### 2. Disable Auto-Detect for Bulk Operations
```csharp
context.ChangeTracker.AutoDetectChangesEnabled = false;
// Add many entities
foreach (var item in items)
{
    context.Items.Add(item);
}
context.ChangeTracker.DetectChanges(); // Detect once
context.SaveChanges();
context.ChangeTracker.AutoDetectChangesEnabled = true;
```

### 3. Clear Tracking for Long-Lived Contexts
```csharp
// After processing a batch
context.ChangeTracker.Clear();
```

### 4. Check What Changed Before Saving
```csharp
if (context.ChangeTracker.HasChanges())
{
    var modified = context.ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Modified);
    // Log or audit changes
}
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
- Show all entity state transitions
- Demonstrate change detection
- Display original vs current values
- Compare tracking vs no-tracking performance
- Provide comprehensive tracking examples

## Learn More

- [Change Tracking in EF Core](https://learn.microsoft.com/en-us/ef/core/change-tracking/)
- [Accessing Tracked Entities](https://learn.microsoft.com/en-us/ef/core/change-tracking/entity-entries)
- [Change Detection and Notifications](https://learn.microsoft.com/en-us/ef/core/change-tracking/change-detection)
- [Identity Resolution](https://learn.microsoft.com/en-us/ef/core/change-tracking/identity-resolution)

## Related Projects

- **01-EFCore-Basics**: EF Core fundamentals
- **02-CodeFirst-Migrations**: Database schema evolution
- **04-Loading-Strategies**: Different ways to load related data
