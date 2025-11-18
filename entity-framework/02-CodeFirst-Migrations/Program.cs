using CodeFirstMigrations.Data;
using CodeFirstMigrations.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeFirstMigrations;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Code First Migrations Demo ===\n");

        // Initialize database
        InitializeDatabase();

        // Demonstrate migration benefits
        DemonstrateSchemaEvolution();

        // Work with migrated data
        DemonstrateDataOperations();

        Console.WriteLine("\n=== Demo Complete ===");
    }

    /// <summary>
    /// Initialize database using migrations
    /// In real applications, use 'dotnet ef database update' instead
    /// </summary>
    static void InitializeDatabase()
    {
        Console.WriteLine("--- Database Initialization ---\n");

        using var context = new ProductContext();

        // EnsureCreated() is used here for demo purposes
        // In real applications with migrations, use: dotnet ef database update
        if (context.Database.EnsureCreated())
        {
            Console.WriteLine("✓ Database created successfully");
        }
        else
        {
            Console.WriteLine("✓ Database already exists");
        }

        // Display migration information
        Console.WriteLine($"✓ Database: {context.Database.GetDbConnection().Database}");
        Console.WriteLine($"✓ Provider: {context.Database.ProviderName}\n");

        // Note about migrations
        Console.WriteLine("NOTE: In a real project with migrations enabled:");
        Console.WriteLine("  1. Run: dotnet ef migrations add InitialCreate");
        Console.WriteLine("  2. Run: dotnet ef database update");
        Console.WriteLine("  3. Make model changes");
        Console.WriteLine("  4. Run: dotnet ef migrations add <DescriptiveName>");
        Console.WriteLine("  5. Run: dotnet ef database update\n");
    }

    /// <summary>
    /// Demonstrates how migrations help evolve database schema
    /// </summary>
    static void DemonstrateSchemaEvolution()
    {
        Console.WriteLine("--- Schema Evolution with Migrations ---\n");

        using var context = new ProductContext();

        Console.WriteLine("Current Product model includes fields added through migrations:");
        Console.WriteLine("  • ProductId, Name, Price, StockQuantity (Initial)");
        Console.WriteLine("  • Description, Category (Migration 2)");
        Console.WriteLine("  • CreatedDate, LastModifiedDate, IsActive (Migration 3)");
        Console.WriteLine("  • SupplierId, Supplier navigation (Migration 4)\n");

        // Show how old data would still work after migrations
        var products = context.Products.Include(p => p.Supplier).ToList();

        Console.WriteLine($"Found {products.Count} products in database:");
        foreach (var product in products)
        {
            Console.WriteLine($"\n  Product: {product.Name}");
            Console.WriteLine($"    Price: ${product.Price}");
            Console.WriteLine($"    Category: {product.Category ?? "N/A"}");
            Console.WriteLine($"    Description: {product.Description ?? "N/A"}");
            Console.WriteLine($"    Created: {product.CreatedDate:yyyy-MM-dd}");
            Console.WriteLine($"    Active: {product.IsActive}");
            Console.WriteLine($"    Supplier: {product.Supplier?.CompanyName ?? "N/A"}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates working with the evolved schema
    /// </summary>
    static void DemonstrateDataOperations()
    {
        Console.WriteLine("--- Data Operations with Migrated Schema ---\n");

        using var context = new ProductContext();

        // Create a new supplier
        var newSupplier = new Supplier
        {
            CompanyName = "Premium Tech Distributors",
            ContactName = "Emily Rodriguez",
            Email = "emily@premiumtech.com",
            Phone = "555-0103",
            Address = "789 Business Blvd, Seattle, WA"
        };

        context.Suppliers.Add(newSupplier);
        context.SaveChanges();
        Console.WriteLine($"✓ Created new supplier: {newSupplier.CompanyName}");

        // Create a product using all fields (including those added via migrations)
        var newProduct = new Product
        {
            Name = "USB-C Hub",
            Price = 49.99m,
            StockQuantity = 75,
            Description = "7-in-1 USB-C hub with HDMI and card readers",
            Category = "Accessories",
            CreatedDate = DateTime.Now,
            IsActive = true,
            SupplierId = newSupplier.SupplierId
        };

        context.Products.Add(newProduct);
        context.SaveChanges();
        Console.WriteLine($"✓ Created new product: {newProduct.Name}");

        // Update product (demonstrating fields from different migration phases)
        var productToUpdate = context.Products.First(p => p.Name == "Mouse");
        productToUpdate.Price = 24.99m; // Original field
        productToUpdate.Description = "Updated: Ergonomic wireless mouse"; // Added in Migration 2
        productToUpdate.LastModifiedDate = DateTime.Now; // Added in Migration 3
        context.SaveChanges();
        Console.WriteLine($"✓ Updated product: {productToUpdate.Name}");

        // Query using fields from different migrations
        Console.WriteLine("\n--- Query Examples ---");

        // Using Category (added in Migration 2)
        var accessories = context.Products
            .Where(p => p.Category == "Accessories")
            .Count();
        Console.WriteLine($"✓ Accessories count (using Category field): {accessories}");

        // Using IsActive (added in Migration 3)
        var activeProducts = context.Products
            .Where(p => p.IsActive)
            .Count();
        Console.WriteLine($"✓ Active products (using IsActive field): {activeProducts}");

        // Using Supplier relationship (added in Migration 4)
        var productsWithSuppliers = context.Products
            .Include(p => p.Supplier)
            .Where(p => p.Supplier != null)
            .Select(p => new { p.Name, SupplierName = p.Supplier!.CompanyName })
            .ToList();

        Console.WriteLine($"✓ Products with suppliers (using relationship):");
        foreach (var item in productsWithSuppliers)
        {
            Console.WriteLine($"    • {item.Name} from {item.SupplierName}");
        }

        // Demonstrate filtering by date (Migration 3 field)
        var recentProducts = context.Products
            .Where(p => p.CreatedDate >= DateTime.Now.AddMonths(-4))
            .OrderByDescending(p => p.CreatedDate)
            .Select(p => new { p.Name, p.CreatedDate })
            .ToList();

        Console.WriteLine($"\n✓ Products created in last 4 months:");
        foreach (var item in recentProducts)
        {
            Console.WriteLine($"    • {item.Name} (Created: {item.CreatedDate:yyyy-MM-dd})");
        }
    }
}
