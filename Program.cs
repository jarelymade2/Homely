using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums;
using StayGo.Models.ValueObjects;
using StayGo.Integration;
using StayGo.Integration; // Solo OpenWeatherIntegration y UnsplashIntegration

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// -----------------
// Connection string
// -----------------
// 1.1. Contexto de la Base de Datos
var connectionString = builder.Configuration.GetConnectionString("StayGoContext")
    ?? throw new InvalidOperationException("Connection string 'StayGoContext' not found.");

builder.Services.AddDbContext<StayGoContext>(options =>
    options.UseSqlite(connectionString));

// -----------------
// Identity (with Roles)
// -----------------
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
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<StayGoContext>();

    .AddRoles<IdentityRole>() // Habilita roles (Admin, etc.)
    .AddEntityFrameworkStores<StayGoContext>();

// 1.3. Autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// -----------------
// Session (VERY IMPORTANT)
// -----------------
// Necesario para HttpContext.Session en tus controladores (ej. historial)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // útil si tienes GDPR / consentimiento
    // options.Cookie.SameSite = SameSiteMode.Lax; // opcional
});

// -----------------
// OpenWeatherIntegration registration
// -----------------
// Registramos como servicio y como HttpClient (typed client)
builder.Services.AddHttpClient<OpenWeatherIntegration>();
builder.Services.AddScoped<OpenWeatherIntegration>();

// -----------------
// MercadoPagoIntegration registration
// -----------------
builder.Services.AddScoped<MercadoPagoIntegration>();

var app = builder.Build();

// -----------------
// Seed Database
// -----------------
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
        await Seed.SeedAsync(services);
        await StayGo.Data.Seed.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// -----------------
// Pipeline
// -----------------
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

// IMPORTANTE: Session debe registrarse en la pipeline antes de ejecutar los endpoints.
// Colocamos UseSession() aquí, después de UseRouting().
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// RUTA PARA ÁREAS (Admin)
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

// Ruta MVC por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
