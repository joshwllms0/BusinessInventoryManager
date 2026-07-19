using BusinessInventoryManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BusinessInventoryManager.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Product> Products => Set<Product>();

        public DbSet<InventoryTransaction> InventoryTransactions =>
            Set<InventoryTransaction>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Explicit primary keys
            builder.Entity<Department>()
                .HasKey(department => department.DepartmentId);

            builder.Entity<Category>()
                .HasKey(category => category.CategoryId);

            builder.Entity<Brand>()
                .HasKey(brand => brand.BrandId);


            builder.Entity<Product>()
                .HasKey(product => product.ProductId);

            builder.Entity<InventoryTransaction>()
                .HasKey(transaction => transaction.TransactionId);

            // Employee-manager self-reference
            builder.Entity<ApplicationUser>()
                .HasOne(user => user.Manager)
                .WithMany(manager => manager.Subordinates)
                .HasForeignKey(user => user.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee-department relationship
            builder.Entity<ApplicationUser>()
                .HasOne(user => user.Department)
                .WithMany(department => department.Employees)
                .HasForeignKey(user => user.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Department-category relationship
            builder.Entity<Category>()
                .HasOne(category => category.Department)
                .WithMany(department => department.Categories)
                .HasForeignKey(category => category.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product-brand relationship
            builder.Entity<Product>()
                .HasOne(product => product.Brand)
                .WithMany(brand => brand.Products)
                .HasForeignKey(product => product.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product-category relationship
            builder.Entity<Product>()
                .HasOne(product => product.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);



            // Unique values
            builder.Entity<Department>()
                .HasIndex(department => department.DepartmentName)
                .IsUnique();

            builder.Entity<Category>()
                .HasIndex(category => new
                {
                    category.DepartmentId,
                    category.CategoryName
                })
                .IsUnique();

            builder.Entity<Brand>()
                .HasIndex(brand => brand.BrandName)
                .IsUnique();

            builder.Entity<Product>()
                .HasIndex(product => product.SKU)
                .IsUnique();

            builder.Entity<Product>()
                .Property(product => product.UnitPrice)
                .HasPrecision(10, 2);

            // Inventory transaction relationships
            builder.Entity<InventoryTransaction>()
                .HasOne(transaction => transaction.Employee)
                .WithMany(employee => employee.InventoryTransactions)
                .HasForeignKey(transaction => transaction.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryTransaction>()
                .HasOne(transaction => transaction.Product)
                .WithMany(product => product.Transactions)
                .HasForeignKey(transaction => transaction.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}