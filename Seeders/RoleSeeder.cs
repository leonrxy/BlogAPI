using BlogAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace BlogAPI.Seeders
{
    public static class RoleUserSeeder
    {
        public static async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Users>>();

            string[] roleNames = { "superadmin", "admin", "user" };

            foreach (var role in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Optional: create admin user
            var adminUser = await userManager.FindByEmailAsync("superadmin@email.com");
            if (adminUser == null)
            {
                var user = new Users
                {
                    UserName = "superadmin@mail.com",
                    Email = "superadmin@mail.com",
                    FullName = "Super Admin"
                };

                var result = await userManager.CreateAsync(user, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "superadmin");
                }
            }
        }
    }
}
