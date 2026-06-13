using BusinessInventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessInventoryManager.Data
{
    public class InventoryContext : DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options)
            : base(options)
        {
        }

        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Manager)
                .WithMany(e => e.Subordinates)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}