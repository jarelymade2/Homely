using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Servicios
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Connection string
var connectionString = builder.Configuration.GetConnectionString("StayGoContext")
    ?? throw new InvalidOperationException("Connection string 'StayGoContext' not found.");

builder.Services.AddDbContext<StayGoContext>(options =>
    options.UseSqlite(connectionString));

// ✅ Configuración de ASP.NET Identity
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// ⬇️ SECCIÓN PARA INICIALIZAR LA BASE DE DATOS Y ROLES
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 🚨 Define las credenciales del Administrador
        const string adminEmail = "StayGo@usmp.pe"; 
        const string adminPassword = "12345678";
        
        // Llama al método de inicialización del Seed
        await Seed.Initialize(services, adminEmail, adminPassword);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos y los roles.");
    }
}
// ⬆️ FIN DE LA SECCIÓN DE INICIALIZACIÓN

// Configuración del Middleware
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

// MIDDLEWARE DE AUTENTICACIÓN Y AUTORIZACIÓN (El orden es clave)
app.UseAuthentication();
app.UseAuthorization();

// ✅ RUTA PARA ÁREAS (Admin) – antes que la default
app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Admin}/{action=Index}/{id?}");

// Ruta MVC por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
