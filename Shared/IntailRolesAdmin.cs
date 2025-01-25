using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Models;

namespace Shared
{
    public static class IntailRolesAdmin
    {        
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = ["Admin", "User", "Supervisor"];

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                   await roleManager.CreateAsync(new IdentityRole(roleName));                
            }
        }
        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();    

            #region 1st Admin Data
            string adminEmail = "ahmedmedhatt2112@gmail.com";
            string adminPassword = "@Hmed1998";
            string fullName = "Ahmed Medhat";
            #endregion

            var user = await userManager.FindByEmailAsync(adminEmail);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = fullName
                };
                await userManager.CreateAsync(user, adminPassword);
            }

            if (!await userManager.IsInRoleAsync(user, "Admin"))            
                await userManager.AddToRoleAsync(user, "Admin");   
        }
    }
}
