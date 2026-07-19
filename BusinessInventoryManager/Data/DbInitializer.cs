using BusinessInventoryManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BusinessInventoryManager.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider services)
        {
            using IServiceScope scope = services.CreateScope();

            ApplicationDbContext context =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            UserManager<ApplicationUser> userManager =
                scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();

            RoleManager<IdentityRole> roleManager =
                scope.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            // Seed grocery lookup data before creating users so that
            // departments are available for future employee assignments.
            await SeedDepartmentsAsync(context);
            await SeedCategoriesAsync(context);
            await SeedBrandsAsync(context);

            await SeedRolesAsync(roleManager);
            await SeedManagerAsync(userManager);
        }

        private static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames =
            {
                "Manager",
                "Employee"
            };

            foreach (string roleName in roleNames)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                {
                    continue;
                }

                IdentityResult roleResult =
                    await roleManager.CreateAsync(
                        new IdentityRole(roleName));

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Unable to create role '{roleName}': " +
                        FormatIdentityErrors(roleResult));
                }
            }
        }

        private static async Task SeedManagerAsync(
            UserManager<ApplicationUser> userManager)
        {
            const string managerEmail =
                "manager@inventory.local";

            ApplicationUser? manager =
                await userManager.FindByEmailAsync(managerEmail);

            if (manager == null)
            {
                string developmentPassword =
                    Environment.GetEnvironmentVariable(
                        "INVENTORY_MANAGER_PASSWORD")
                    ?? throw new InvalidOperationException(
                        "INVENTORY_MANAGER_PASSWORD is not configured. " +
                        "Export the variable before running the application.");

                manager = new ApplicationUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    FirstName = "System",
                    LastName = "Manager",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                IdentityResult createResult =
                    await userManager.CreateAsync(
                        manager,
                        developmentPassword);

                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Unable to create the seeded manager account: " +
                        FormatIdentityErrors(createResult));
                }
            }

            // This also repairs the account if the user exists
            // but is not currently assigned to the Manager role.
            if (!await userManager.IsInRoleAsync(manager, "Manager"))
            {
                IdentityResult roleResult =
                    await userManager.AddToRoleAsync(
                        manager,
                        "Manager");

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Unable to assign the seeded manager role: " +
                        FormatIdentityErrors(roleResult));
                }
            }
        }

        private static async Task SeedDepartmentsAsync(
            ApplicationDbContext context)
        {
            var departments = new[]
            {
                new Department
                {
                    DepartmentName = "Produce",
                    Description =
                        "Fresh fruits, vegetables, herbs, salads, " +
                        "and refrigerated produce."
                },
                new Department
                {
                    DepartmentName = "Dairy and Refrigerated",
                    Description =
                        "Milk, cream, cheese, yogurt, eggs, " +
                        "and other refrigerated products."
                },
                new Department
                {
                    DepartmentName = "Frozen Foods",
                    Description =
                        "Frozen meals, vegetables, desserts, " +
                        "and convenience foods."
                },
                new Department
                {
                    DepartmentName = "Pantry and Canned Goods",
                    Description =
                        "Shelf-stable food, canned products, pasta, " +
                        "rice, sauces, and condiments."
                },
                new Department
                {
                    DepartmentName = "Beverages and Snacks",
                    Description =
                        "Soft drinks, water, juice, chips, crackers, " +
                        "and packaged snacks."
                },
                new Department
                {
                    DepartmentName = "Bakery and Deli",
                    Description =
                        "Bread, baked goods, prepared foods, deli meat, " +
                        "and deli cheese."
                }
            };

            List<string> existingDepartmentNames =
                await context.Departments
                    .Select(department => department.DepartmentName)
                    .ToListAsync();

            HashSet<string> existingNames =
                existingDepartmentNames.ToHashSet(
                    StringComparer.OrdinalIgnoreCase);
            IEnumerable<Department> missingDepartments =
                departments.Where(department =>
                    !existingNames.Contains(
                        department.DepartmentName));

            context.Departments.AddRange(missingDepartments);

            await context.SaveChangesAsync();
        }

        private static async Task SeedCategoriesAsync(
            ApplicationDbContext context)
        {
            Dictionary<string, int> departmentIds =
                await context.Departments
                    .ToDictionaryAsync(
                        department => department.DepartmentName,
                        department => department.DepartmentId);

            var categorySeeds = new[]
            {
                new CategorySeed("Produce", "Fresh Fruit"),
                new CategorySeed("Produce", "Fresh Vegetables"),
                new CategorySeed("Produce", "Herbs and Salad"),

                new CategorySeed(
                    "Dairy and Refrigerated",
                    "Milk and Cream"),
                new CategorySeed(
                    "Dairy and Refrigerated",
                    "Cheese"),
                new CategorySeed(
                    "Dairy and Refrigerated",
                    "Yogurt"),
                new CategorySeed(
                    "Dairy and Refrigerated",
                    "Eggs"),

                new CategorySeed(
                    "Frozen Foods",
                    "Frozen Meals"),
                new CategorySeed(
                    "Frozen Foods",
                    "Frozen Vegetables"),
                new CategorySeed(
                    "Frozen Foods",
                    "Ice Cream"),

                new CategorySeed(
                    "Pantry and Canned Goods",
                    "Canned Goods"),
                new CategorySeed(
                    "Pantry and Canned Goods",
                    "Pasta and Rice"),
                new CategorySeed(
                    "Pantry and Canned Goods",
                    "Sauces and Condiments"),

                new CategorySeed(
                    "Beverages and Snacks",
                    "Soft Drinks"),
                new CategorySeed(
                    "Beverages and Snacks",
                    "Water and Juice"),
                new CategorySeed(
                    "Beverages and Snacks",
                    "Chips and Snacks"),

                new CategorySeed(
                    "Bakery and Deli",
                    "Bread and Baked Goods"),
                new CategorySeed(
                    "Bakery and Deli",
                    "Deli Meat and Cheese")
            };

            var existingCategories =
                await context.Categories
                    .Select(category => new
                    {
                        category.DepartmentId,
                        category.CategoryName
                    })
                    .ToListAsync();

            foreach (CategorySeed seed in categorySeeds)
            {
                if (!departmentIds.TryGetValue(
                    seed.DepartmentName,
                    out int departmentId))
                {
                    throw new InvalidOperationException(
                        $"The department '{seed.DepartmentName}' " +
                        "was not found while seeding categories.");
                }

                bool alreadyExists =
                    existingCategories.Any(category =>
                        category.DepartmentId == departmentId &&
                        category.CategoryName == seed.CategoryName);

                if (alreadyExists)
                {
                    continue;
                }

                context.Categories.Add(
                    new Category
                    {
                        DepartmentId = departmentId,
                        CategoryName = seed.CategoryName,
                        Description = seed.Description
                    });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedBrandsAsync(
            ApplicationDbContext context)
        {
            string[] brandNames =
            {
                "Store Select",
                "Coca-Cola",
                "Pepsi",
                "Kellogg's",
                "General Mills",
                "Kraft",
                "Heinz",
                "Campbell's",
                "Del Monte",
                "Dole",
                "Chobani",
                "Yoplait",
                "Dannon",
                "Land O'Lakes",
                "Kemps",
                "Sargento",
                "Tyson",
                "Oscar Mayer",
                "Doritos",
                "Lay's",
                "Nabisco",
                "Ben & Jerry's",
                "DiGiorno",
                "Birds Eye"
            };

            List<string> existingBrandNameList =
                await context.Brands
                    .Select(brand => brand.BrandName)
                    .ToListAsync();

            HashSet<string> existingBrandNames =
                existingBrandNameList.ToHashSet(
                    StringComparer.OrdinalIgnoreCase);
            IEnumerable<Brand> missingBrands =
                brandNames
                    .Where(brandName =>
                        !existingBrandNames.Contains(brandName))
                    .Select(brandName =>
                        new Brand
                        {
                            BrandName = brandName
                        });

            context.Brands.AddRange(missingBrands);

            await context.SaveChangesAsync();
        }
    
        private static string FormatIdentityErrors(
            IdentityResult result)
        {
            return string.Join(
                "; ",
                result.Errors.Select(error =>
                    error.Description));
        }

        private sealed record CategorySeed(
            string DepartmentName,
            string CategoryName,
            string? Description = null);

}
}