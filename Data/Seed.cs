using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // Necesario para EnsureCreated/Migrate
using StayGo.Models;
using StayGo.Models.Enums;
using StayGo.Models.ValueObjects;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace StayGo.Data
{
    public class Seed
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {

            var context = serviceProvider.GetRequiredService<StayGoContext>();


            await context.Database.MigrateAsync();


            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Crear Roles
            string[] roleNames = { "Admin", "User", "Guest" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Crear usuario Admin
            const string adminEmail = "admin@staygo.com";
            const string adminPassword = "password123!";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "StayGo",
                    EmailConfirmed = true
                };

                // Crear el usuario y hashear la contraseña
                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    // Asignar el rol 'Admin'
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    // Opcional: Loggear si la creación del usuario falla por validación (ej. reglas de contraseña)
                    throw new Exception($"Failed to create Admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // 3. Crear Amenidades
            if (!context.Amenidades.Any())
            {
                var amenidades = new List<Amenidad>
                {
                    new Amenidad { Nombre = "WiFi" },
                    new Amenidad { Nombre = "Piscina" },
                    new Amenidad { Nombre = "Estacionamiento" },
                    new Amenidad { Nombre = "Aire Acondicionado" },
                    new Amenidad { Nombre = "Cocina" },
                    new Amenidad { Nombre = "TV" },
                    new Amenidad { Nombre = "Gimnasio" },
                    new Amenidad { Nombre = "Mascotas Permitidas" }
                };
                context.Amenidades.AddRange(amenidades);
                await context.SaveChangesAsync();
            }
        }
    }
}
