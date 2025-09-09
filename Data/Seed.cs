using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums;

public static class Seed
{
    public static async Task RunAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<StayGoContext>();
        await ctx.Database.MigrateAsync();

        // Admin Identity (si ya tienes Identity configurado en Program.cs)
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var r in new[] { "Admin", "Cliente" })
            if (!await roles.RoleExistsAsync(r)) await roles.CreateAsync(new IdentityRole(r));

        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var adminEmail = "admin@staygo.local";
        var admin = await users.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await users.CreateAsync(admin, "Admin123!");
            await users.AddToRoleAsync(admin, "Admin");
        }

        // Vinculamos IdentityUser con nuestro Usuario de dominio
        var adminUsuario = await ctx.Usuarios.FirstOrDefaultAsync(u => u.IdentityUserId == admin.Id);
        if (adminUsuario is null)
        {
            adminUsuario = new Usuario { Id = Guid.NewGuid(), Email = adminEmail, IdentityUserId = admin.Id, EsAdmin = true, Nombre = "Admin" };
            ctx.Usuarios.Add(adminUsuario);
        }

        // Amenidades base
        if (!await ctx.Amenidades.AnyAsync())
        {
            ctx.Amenidades.AddRange(
                new Amenidad { Nombre = "Wi-Fi" },
                new Amenidad { Nombre = "Estacionamiento" },
                new Amenidad { Nombre = "Piscina" },
                new Amenidad { Nombre = "Desayuno" }
            );
        }

        // Propiedades demo
        if (!await ctx.Propiedades.AnyAsync())
        {
            var casa = new Propiedad
            {
                Id = Guid.NewGuid(),
                Titulo = "Casa de Playa",
                Descripcion = "Vista al mar, perfecta para fines de semana.",
                Tipo = TipoPropiedad.Casa,
                PrecioPorNoche = 350,
                Capacidad = 6,
                Lat = -12.782, Lng = -76.632,
                Direccion = new() { Pais = "Perú", Ciudad = "Asia", Linea1 = "Km 97.5 Panamericana Sur" }
            };
            var hotel = new Propiedad
            {
                Id = Guid.NewGuid(),
                Titulo = "Hotel Miraflores",
                Descripcion = "Hotel céntrico con desayuno.",
                Tipo = TipoPropiedad.Hotel,
                PrecioPorNoche = null,
                Capacidad = 100,
                Lat = -12.122, Lng = -77.030,
                Direccion = new() { Pais = "Perú", Ciudad = "Lima", Linea1 = "Av. Pardo 123" }
            };
            var hab1 = new Habitacion { Id = Guid.NewGuid(), Propiedad = hotel, Nombre = "Doble 201", Capacidad = 2, PrecioPorNoche = 180, TipoCama = TipoCama.Doble };
            var hab2 = new Habitacion { Id = Guid.NewGuid(), Propiedad = hotel, Nombre = "Queen 305", Capacidad = 2, PrecioPorNoche = 220, TipoCama = TipoCama.Queen };

            ctx.Propiedades.AddRange(casa, hotel);
            ctx.Habitaciones.AddRange(hab1, hab2);

            // Imagenes
            ctx.ImagenesPropiedad.AddRange(
                new ImagenPropiedad { Id = Guid.NewGuid(), Propiedad = casa, Url = "/img/demo1.jpg", EsPrincipal = true },
                new ImagenPropiedad { Id = Guid.NewGuid(), Propiedad = hotel, Url = "/img/demo2.jpg", EsPrincipal = true }
            );

            await ctx.SaveChangesAsync();

            // Amenidades link
            var wifi = await ctx.Amenidades.FirstAsync(a => a.Nombre == "Wi-Fi");
            ctx.PropiedadAmenidades.Add(new PropiedadAmenidad { PropiedadId = casa.Id, AmenidadId = wifi.Id });
            ctx.PropiedadAmenidades.Add(new PropiedadAmenidad { PropiedadId = hotel.Id, AmenidadId = wifi.Id });

            // Reservas + Reseña
            var reserva = new Reserva
            {
                Id = Guid.NewGuid(),
                PropiedadId = casa.Id,
                Usuario = adminUsuario,
                CheckIn = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(7)),
                CheckOut = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(10)),
                Huespedes = 4,
                PrecioTotal = 350 * 3,
            };
            ctx.Reservas.Add(reserva);

            ctx.Resenas.Add(new Resena
            {
                Id = Guid.NewGuid(),
                PropiedadId = casa.Id,
                UsuarioId = adminUsuario.Id,
                Puntuacion = 5,
                Comentario = "Excelente estadía.",
                Fecha = DateTime.UtcNow
            });
        }

        await ctx.SaveChangesAsync();
    }
}
