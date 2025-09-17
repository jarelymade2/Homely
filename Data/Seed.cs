using Microsoft.AspNetCore.Identity;
using StayGo.Models; // Asegúrate de que esta línea esté aquí
using StayGo.Data;
using System;
using System.Threading.Tasks;

namespace StayGo.Data;

public class Seed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var context = serviceProvider.GetRequiredService<StayGoContext>();

        // Crear roles si no existen
        string[] roleNames = { "Admin", "User" };
        IdentityResult roleResult;

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Crear un usuario de prueba si no existe
        if (userManager.FindByEmailAsync("admin@staygo.com").Result == null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin@staygo.com",
                Email = "admin@staygo.com",
                FirstName = "Admin",
                LastName = "StayGo",
                PhoneNumber = "123456789"
            };

            var result = await userManager.CreateAsync(user, "password123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}