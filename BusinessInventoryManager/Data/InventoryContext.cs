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
    }
}