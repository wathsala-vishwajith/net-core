# Migration Instructions

This document explains how to work with migrations in this project.

## What are Migrations?

Migrations provide a way to incrementally update the database schema to keep it in sync with your application's data model while preserving existing data in the database.

## Common Migration Commands

### 1. Add a New Migration

When you make changes to your model classes, create a migration:

```bash
dotnet ef migrations add <MigrationName>
```

Example:
```bash
dotnet ef migrations add InitialCreate
dotnet ef migrations add AddProductDescription
dotnet ef migrations add AddSupplierEntity
```

### 2. Update Database

Apply pending migrations to the database:

```bash
dotnet ef database update
```

### 3. Update to Specific Migration

Roll back or forward to a specific migration:

```bash
dotnet ef database update <MigrationName>
```

Example:
```bash
dotnet ef database update InitialCreate
```

### 4. Remove Last Migration

Remove the last migration (only if not applied to database):

```bash
dotnet ef migrations remove
```

### 5. List All Migrations

See all migrations and their status:

```bash
dotnet ef migrations list
```

### 6. Generate SQL Script

Generate SQL script for migrations:

```bash
dotnet ef migrations script
```

Generate SQL for specific migration range:

```bash
dotnet ef migrations script <FromMigration> <ToMigration>
```

### 7. Drop Database

Delete the database:

```bash
dotnet ef database drop
```

## Migration Workflow Example

### Scenario: Evolution of Product Model

#### Migration 1: Initial Create
```csharp
// Product.cs - Version 1
public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
```

Command:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### Migration 2: Add Description and Category
```csharp
// Product.cs - Version 2
public class Product
{
    // ... existing properties
    public string? Description { get; set; }
    public string? Category { get; set; }
}
```

Command:
```bash
dotnet ef migrations add AddProductDetailsFields
dotnet ef database update
```

#### Migration 3: Add Audit Fields
```csharp
// Product.cs - Version 3
public class Product
{
    // ... existing properties
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public bool IsActive { get; set; }
}
```

Command:
```bash
dotnet ef migrations add AddAuditFields
dotnet ef database update
```

#### Migration 4: Add Supplier Relationship
```csharp
// Create new Supplier.cs
public class Supplier
{
    public int SupplierId { get; set; }
    public string CompanyName { get; set; }
    // ... other properties
}

// Update Product.cs - Version 4
public class Product
{
    // ... existing properties
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
}
```

Command:
```bash
dotnet ef migrations add AddSupplierEntity
dotnet ef database update
```

## Migration File Structure

After running migrations, you'll see files like:

```
Migrations/
├── 20231118000001_InitialCreate.cs
├── 20231118000001_InitialCreate.Designer.cs
├── 20231118000002_AddProductDetailsFields.cs
├── 20231118000002_AddProductDetailsFields.Designer.cs
├── 20231119000001_AddAuditFields.cs
├── 20231119000001_AddAuditFields.Designer.cs
├── 20231120000001_AddSupplierEntity.cs
├── 20231120000001_AddSupplierEntity.Designer.cs
└── ProductContextModelSnapshot.cs
```

## Understanding Migration Files

### Migration Class (.cs)
Contains two methods:
- `Up()`: Changes to apply when migrating forward
- `Down()`: Changes to revert when rolling back

Example:
```csharp
public partial class AddProductDescription : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Products",
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

### Model Snapshot
`ProductContextModelSnapshot.cs` contains the current state of your model. EF Core uses this to determine what changes need to be made when creating new migrations.

## Custom Migration Operations

You can customize migrations by editing the generated files:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Generated code
    migrationBuilder.AddColumn<decimal>(
        name: "Price",
        table: "Products",
        nullable: false,
        defaultValue: 0m);

    // Custom SQL
    migrationBuilder.Sql(
        "UPDATE Products SET Price = 9.99 WHERE Price = 0");

    // Create index
    migrationBuilder.CreateIndex(
        name: "IX_Products_Category",
        table: "Products",
        column: "Category");
}
```

## Data Migration

For complex data transformations:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Add new column
    migrationBuilder.AddColumn<string>(
        name: "FullName",
        table: "Customers",
        nullable: true);

    // Migrate data
    migrationBuilder.Sql(@"
        UPDATE Customers
        SET FullName = FirstName + ' ' + LastName
    ");

    // Make it required
    migrationBuilder.AlterColumn<string>(
        name: "FullName",
        table: "Customers",
        nullable: false);
}
```

## Best Practices

1. **Descriptive Names**: Use clear migration names that describe the change
   - ✅ `AddUserEmailField`
   - ❌ `UpdateModel`

2. **Small Migrations**: Keep migrations focused on a single logical change

3. **Test Rollbacks**: Always test the `Down()` method works correctly

4. **Review Generated Code**: Check the migration before applying it

5. **Source Control**: Commit migrations with the code changes

6. **Production**:
   - Generate SQL scripts for production deployments
   - Test migrations on a copy of production data
   - Have a rollback plan

7. **Don't Modify Applied Migrations**: Once a migration is applied (especially in production), don't modify it. Create a new migration instead.

## Troubleshooting

### "Migration already applied"
```bash
# Remove from database without running Down()
dotnet ef database update <PreviousMigration>
# Then remove the migration file
dotnet ef migrations remove
```

### "No migrations configuration type found"
Ensure you have:
- `Microsoft.EntityFrameworkCore.Design` package installed
- A DbContext with proper configuration

### "Build failed"
Fix compilation errors before creating migrations

### "Database in use"
Close all connections to the database before dropping it

## Additional Resources

- [EF Core Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Migrations with Multiple Providers](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers)
- [Custom Migration Operations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/operations)
