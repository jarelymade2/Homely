using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;

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
        // Reglas de Contraseña (coincide con tu configuración)
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;
    })
    .AddRoles<IdentityRole>() // Habilita el soporte para roles (necesario para tu Seed y Autorización)
    .AddEntityFrameworkStores<StayGoContext>();

// 1.3. Autorización (incluye tu política 'AdminOnly')
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// =========================================================
// 2. CONSTRUCCIÓN DE LA APLICACIÓN
// =========================================================

var app = builder.Build();

// 2.1. Ejecución de la Siembra de Datos (Seed)
// Esto crea los roles y al usuario "admin@staygo.com" si no existen
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Llama al método SeedAsync para inicializar datos
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

// Middleware de Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// 3.1. Rutas (Routing)
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

app.MapRazorPages(); // Necesario para las páginas de Identity (Login, Register, etc.)

app.Run();