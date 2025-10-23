using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums;
using StayGo.Models.ValueObjects;
using StayGo.Integration; // Solo OpenWeatherIntegration y UnsplashIntegration

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. CONFIGURACIÓN DE SERVICIOS
// =========================================================

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 1.1. Contexto de la Base de Datos
var connectionString = builder.Configuration.GetConnectionString("StayGoContext")
    ?? throw new InvalidOperationException("Connection string 'StayGoContext' not found.");

builder.Services.AddDbContext<StayGoContext>(options =>
    options.UseSqlite(connectionString));

// 1.2. Configuración de Identity (con ApplicationUser y Roles)
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;
    })
    .AddRoles<IdentityRole>() // Habilita roles (Admin, etc.)
    .AddEntityFrameworkStores<StayGoContext>();

// 1.3. Autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// =========================================================
// 🌤️ Registro de INTEGRACIONES (APIs externas)
// =========================================================

// 1️⃣ OpenWeather (ya lo tienes)
builder.Services.AddScoped<OpenWeatherIntegration>();

// 2️⃣ Unsplash (para imágenes de propiedades)
builder.Services.AddScoped<UnsplashIntegration>();

// =========================================================
// 2. CONSTRUCCIÓN DE LA APLICACIÓN
// =========================================================

var app = builder.Build();

// 2.1. Ejecución del Seed (roles y admin)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await StayGo.Data.Seed.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// =========================================================
// 3. PIPELINE DE SOLICITUDES HTTP
// =========================================================

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// =========================================================
// 4. RUTAS
// =========================================================

app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Admin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
