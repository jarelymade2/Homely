using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Services; // 🛑 Importar el namespace del servicio
using Microsoft.AspNetCore.Identity.UI.Services; // 🛑 Importar IEmailSender

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. CONFIGURACIÓN DE SERVICIOS
// =========================================================

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configuración de Sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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
        // Reglas de Contraseña
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<StayGoContext>();

// 🛑 1.3. SERVICIO DE ENVÍO DE CORREO REAL (SendGrid)
// Esta sección inyecta tu lógica de correo para que Identity la use
// para el restablecimiento de contraseñas.

// A. Configura las opciones de SendGrid leyendo appsettings.json
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration.GetSection("SendGridOptions"));

// B. Reemplaza el servicio de correo por defecto (ficticio) con tu implementación real
builder.Services.AddTransient<IEmailSender, EmailSender>();


// 1.4. Autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// =========================================================
// 2. CONSTRUCCIÓN DE LA APLICACIÓN
// =========================================================

var app = builder.Build();

// 2.1. Ejecución de la Siembra de Datos (Seed)
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

// Middleware de Session (DEBE IR AQUÍ)
app.UseSession();

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

app.MapRazorPages();

app.Run();