using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.ValueObjects;
using StayGo.Models.Enums;  



public static class Seed
{
    public static async Task RunAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var services = scope.ServiceProvider;

        var ctx  = services.GetRequiredService<StayGoContext>();
        var um   = services.GetRequiredService<UserManager<IdentityUser>>();
        var rm   = services.GetRequiredService<RoleManager<IdentityRole>>();

        // 🔹 Asegura DB y aplica migraciones
        await ctx.Database.MigrateAsync();

        // 🔹 Roles
        foreach (var role in new[] { "Admin", "Cliente" })
        {
            if (!await rm.RoleExistsAsync(role))
                await rm.CreateAsync(new IdentityRole(role));
        }

        // 🔹 Usuario admin (Identity)
        const string adminEmail = "admin@staygo.local";
        const string adminPass  = "Admin123!";
        var admin = await um.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            await um.CreateAsync(admin, adminPass);
            await um.AddToRoleAsync(admin, "Admin");
        }

        // 🔹 Usuario de dominio vinculado a Identity
        var adminUsuario = await ctx.Usuarios.FirstOrDefaultAsync(u => u.IdentityUserId == admin.Id);
        if (adminUsuario is null)
        {
            adminUsuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                Nombre = "Admin",
                IdentityUserId = admin.Id,
                EsAdmin = true
            };
            ctx.Usuarios.Add(adminUsuario);
        }

        // 🔹 Amenidades base
        if (!await ctx.Amenidades.AnyAsync())
        {
            ctx.Amenidades.AddRange(
                new Amenidad { Nombre = "Wi-Fi" },
                new Amenidad { Nombre = "Estacionamiento" },
                new Amenidad { Nombre = "Piscina" },
                new Amenidad { Nombre = "Desayuno" }
            );
        }

        // 🔹 Propiedades demo
        if (!await ctx.Propiedades.AnyAsync())
        {
            var casa = new Propiedad
            {
                Id = Guid.NewGuid(),
                Titulo = "Casa de Playa",
                Descripcion = "Vista al mar, perfecta para fines de semana.",
                Tipo = TipoPropiedad.Casa,
                PrecioPorNoche = 350m,
                Capacidad = 6,
                Lat = -12.782, Lng = -76.632,
                Direccion = new Direccion
                {
                    Pais = "Perú",
                    Ciudad = "Asia",
                    Linea1 = "Km 97.5 Panamericana Sur"
                },
                Imagenes =
                {
                    new ImagenPropiedad { Id = Guid.NewGuid(), Url = "/img/demo1.jpg", EsPrincipal = true }
                }
            };

            var hotel = new Propiedad
            {
                Id = Guid.NewGuid(),
                Titulo = "Hotel Miraflores",
                Descripcion = "Hotel céntrico con desayuno.",
                Tipo = TipoPropiedad.Hotel,
                PrecioPorNoche = null, // hotel: precio por habitación
                Capacidad = 100,
                Lat = -12.122, Lng = -77.030,
                Direccion = new Direccion
                {
                    Pais = "Perú",
                    Ciudad = "Lima",
                    Linea1 = "Av. Pardo 123"
                },
                Imagenes =
                {
                    new ImagenPropiedad { Id = Guid.NewGuid(), Url = "/img/demo2.jpg", EsPrincipal = true }
                },
                Habitaciones =
                {
                    new Habitacion { Id = Guid.NewGuid(), Nombre = "Doble 201", Capacidad = 2, PrecioPorNoche = 180m, TipoCama = TipoCama.Doble },
                    new Habitacion { Id = Guid.NewGuid(), Nombre = "Queen 305", Capacidad = 2, PrecioPorNoche = 220m, TipoCama = TipoCama.Queen }
                }
            };

            ctx.Propiedades.AddRange(casa, hotel);
            await ctx.SaveChangesAsync();

            // 🔹 Amenidades ↔ Propiedades
            var wifi = await ctx.Amenidades.FirstAsync(a => a.Nombre == "Wi-Fi");
            ctx.PropiedadAmenidades.AddRange(
                new PropiedadAmenidad { PropiedadId = casa.Id,  AmenidadId = wifi.Id },
                new PropiedadAmenidad { PropiedadId = hotel.Id, AmenidadId = wifi.Id }
            );

            // 🔹 Reserva confirmada
            var reserva = new Reserva
            {
                Id = Guid.NewGuid(),
                PropiedadId = casa.Id,
                UsuarioId = adminUsuario.Id,
                CheckIn  = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(7)),
                CheckOut = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(10)),
                Huespedes = 4,
                PrecioTotal = 350m * 3,
                Estado = EstadoReserva.Confirmada
            };
            ctx.Reservas.Add(reserva);

            // 🔹 Reseña
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
