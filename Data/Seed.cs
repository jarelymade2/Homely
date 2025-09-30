using Microsoft.AspNetCore.Identity;
using StayGo.Models; // Asegúrate de que esta línea esté aquí
using System.Threading.Tasks;

namespace StayGo.Data;

public static class Seed
{
    // Usamos static Task, y simplificamos la firma para que coincida con la llamada en Program.cs
    public static async Task Initialize(
        IServiceProvider serviceProvider,
        string adminEmail,
        string adminPassword)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        // var context = serviceProvider.GetRequiredService<StayGoContext>(); // No es necesario si solo manejamos Identity aquí

        // 1. Crear roles si no existen
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Crear usuario Administrador si no existe (StayGo@usmp.pe)
        if (userManager.FindByEmailAsync(adminEmail).Result == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "StayGo",
                LastName = "Admin",
                PhoneNumber = "999999999", // Número de prueba
                EmailConfirmed = true // Para evitar problemas de confirmación al inicio
            };

            // Creamos el usuario con la contraseña
            var result = await userManager.CreateAsync(user, adminPassword);

            if (result.Succeeded)
            {
                // 3. Asignar el rol 'Admin'
                await userManager.AddToRoleAsync(user, "Admin");
            }
            // NOTA: Si la creación falla, el error podría ser por las políticas de contraseña 
            // definidas en Program.cs (aunque las quitaste, es bueno saberlo)
        }
        
    }
}
