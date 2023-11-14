using HUBShop.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace HUBShop.Servises
{
    public class AdminInitial
    {
        public static async Task SeedAdminUser(RoleManager<IdentityRole<int>> roleManager, UserManager<User> userManager)
        {
            string adminEmail = "mur-mur-0998@mail.ru";
            string adminPassword = "123!A123";
            var roles = new[] { "admin", "user" };
            foreach (var r in roles)
            {
                if (await roleManager.FindByNameAsync(r) is null)
                    await roleManager.CreateAsync(new IdentityRole<int>(r));
            }
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                User admin = new User {Email = adminEmail, UserName = adminEmail };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "admin");
            }
        }

    }
}
