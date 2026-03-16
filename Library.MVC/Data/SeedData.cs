using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Library.MVC.Data
{
    public static class SeedData
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            // Get the services 
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Create Admin role if it doesn't exist
            string adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
                Console.WriteLine("Admin role created.");
            }

            // 2. Create Admin user if it doesn't exist
            string adminEmail = "admin@library.com";
            string adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Create new admin user
                var user = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true // Skip email confirmation
                };

                var result = await userManager.CreateAsync(user, adminPassword);

                if (result.Succeeded)
                {
                    // Add user to Admin role
                    await userManager.AddToRoleAsync(user, adminRole);
                    Console.WriteLine("Admin user created and added to Admin role.");
                }
                else
                {
                    // If creation failed, show errors
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error: {error.Description}");
                    }
                }
            }
            else
            {
                // Make sure existing user is in Admin role
                if (!await userManager.IsInRoleAsync(adminUser, adminRole))
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                    Console.WriteLine("Existing user added to Admin role.");
                }
                else
                {
                    Console.WriteLine("Admin user already exists.");
                }
            }
        }
    }
}