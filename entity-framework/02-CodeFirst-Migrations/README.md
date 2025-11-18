# Code First Migrations

This project demonstrates Entity Framework Core's **Code First Migrations** feature, which allows you to evolve your database schema over time while preserving data.

## Overview

Code First Migrations enable you to:
- Create and modify database schema through code
- Track schema changes over time
- Apply or rollback changes to databases
- Handle schema evolution in development and production
- Maintain data integrity during schema updates

## Project Structure

```
02-CodeFirst-Migrations/
├── Data/
│   └── ProductContext.cs           # DbContext with configuration
├── Models/
│   ├── Product.cs                  # Product entity (evolved through migrations)
│   └── Supplier.cs                 # Supplier entity (added in later migration)
├── Migrations/
│   └── Migration_Instructions.md  # Comprehensive migration guide
├── Program.cs                      # Demonstration code
├── CodeFirstMigrations.csproj     # Project file
└── README.md                       # This file
```

## Key Concepts

### 1. Code First Development

With Code First approach:
1. Define your model classes (entities)
2. Create a DbContext
3. EF Core creates the database schema from your code

**Advantages:**
- Full control over your domain model
- Version control for schema changes
- Strong typing and IntelliSense support
- Easier testing and refactoring

### 2. Migrations

Migrations are incremental changes to your database schema:

```
Initial Model → Migration 1 → Migration 2 → Migration 3 → Current State
```

Each migration contains:
- **Up()**: Changes to apply when migrating forward
- **Down()**: Changes to revert when rolling back

### 3. Schema Evolution Example

This project demonstrates a realistic evolution:

**Phase 1: Basic Product Model**
```csharp
public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
```

**Phase 2: Add Business Fields**
```csharp
// Added Description and Category
public string? Description { get; set; }
public string? Category { get; set; }
```

**Phase 3: Add Audit Trail**
```csharp
// Added audit fields
public DateTime CreatedDate { get; set; }
public DateTime? LastModifiedDate { get; set; }
public bool IsActive { get; set; }
```

**Phase 4: Add Relationships**
```csharp
// Added Supplier relationship
public int? SupplierId { get; set; }
public Supplier? Supplier { get; set; }
```

## Working with Migrations

### Creating a Migration

When you modify your model:

```bash
dotnet ef migrations add AddProductDescription
```

This generates:
- `YYYYMMDDHHMMSS_AddProductDescription.cs` - Migration code
- `YYYYMMDDHHMMSS_AddProductDescription.Designer.cs` - Metadata
- Updates `ProductContextModelSnapshot.cs` - Current model state

### Applying Migrations

Update the database to the latest migration:

```bash
dotnet ef database update
```

### Rolling Back

Revert to a specific migration:

```bash
dotnet ef database update PreviousMigrationName
```

### Removing a Migration

Remove the last migration (if not applied):

```bash
dotnet ef migrations remove
```

## Migration Workflow

### Step-by-Step Example

1. **Initial Setup**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

2. **Add New Fields**
   ```csharp
   // Modify Product.cs - add Description field
   public string? Description { get; set; }
   ```
   ```bash
   dotnet ef migrations add AddProductDescription
   dotnet ef database update
   ```

3. **Add New Entity**
   ```csharp
   // Create Supplier.cs
   public class Supplier { ... }

   // Update Product.cs - add relationship
   public Supplier? Supplier { get; set; }
   ```
   ```bash
   dotnet ef migrations add AddSupplierEntity
   dotnet ef database update
   ```

## Understanding Migration Files

### Generated Migration Example

```csharp
public partial class AddProductDescription : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Products",
            type: "TEXT",
            maxLength: 500,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Description",
            table: "Products");
    }
}
```

**Up()**: Adds the Description column
**Down()**: Removes the Description column (for rollback)

## Advanced Migration Scenarios

### Custom SQL in Migrations

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<decimal>(
        name: "Discount",
        table: "Products",
        nullable: false,
        defaultValue: 0m);

    // Custom SQL for data migration
    migrationBuilder.Sql(@"
        UPDATE Products
        SET Discount = 0.1
        WHERE Category = 'Electronics'
    ");
}
```

### Data Seeding

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>().HasData(
        new Product
        {
            ProductId = 1,
            Name = "Laptop",
            Price = 999.99m
        }
    );
}
```

### Handling Renames

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.RenameColumn(
        name: "Price",
        table: "Products",
        newName: "UnitPrice");
}
```

## Best Practices

### 1. Migration Naming
Use descriptive names:
- ✅ `AddProductCategoryField`
- ✅ `CreateSupplierTable`
- ✅ `UpdateProductPriceIndexes`
- ❌ `Update1`
- ❌ `FixStuff`

### 2. Small, Focused Migrations
Each migration should represent a single logical change:
```bash
# Good
dotnet ef migrations add AddUserEmailField
dotnet ef migrations add AddUserPhoneField

# Not ideal
dotnet ef migrations add AddLotsOfUserFields
```

### 3. Test Rollbacks
Always verify Down() works:
```bash
dotnet ef database update PreviousMigration
dotnet ef database update  # Apply again
```

### 4. Review Generated Code
Inspect migrations before applying:
- Check for unintended changes
- Verify data loss warnings
- Add custom SQL if needed

### 5. Production Deployment

**Option 1: SQL Scripts (Recommended)**
```bash
dotnet ef migrations script --output migration.sql
# Review and apply SQL manually
```

**Option 2: Automatic Migration**
```csharp
// Startup.cs
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductContext>();
    context.Database.Migrate();
}
```

### 6. Source Control
- Commit migrations with related code changes
- Never modify applied migrations
- Keep migration history clean

## Common Commands Reference

```bash
# Create migration
dotnet ef migrations add <Name>

# Apply migrations
dotnet ef database update

# List migrations
dotnet ef migrations list

# Generate SQL script
dotnet ef migrations script

# Rollback to specific migration
dotnet ef database update <MigrationName>

# Remove last migration (if not applied)
dotnet ef migrations remove

# Drop database
dotnet ef database drop
```

## Running This Project

1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Create initial migration (if using migrations):**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

The demo will:
- Create/verify the database
- Show schema evolution
- Demonstrate CRUD operations using fields from different migration phases
- Display how old and new data work together

## Key Takeaways

1. **Migrations track schema changes** over time
2. **Each migration is versioned** and reversible
3. **Data is preserved** during schema updates
4. **Migrations work in teams** - share via source control
5. **Production requires careful planning** - use SQL scripts or automated migrations with backups

## Learn More

- [EF Core Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Managing Migration Files](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing)
- [Team Environments](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/teams)
- [Custom Operations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/operations)

## Related Projects

- **01-EFCore-Basics**: Understanding EF Core fundamentals
- **03-ChangeTracker-API**: How EF tracks entity changes
- **04-Loading-Strategies**: Optimizing data loading
