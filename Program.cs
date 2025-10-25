using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums;
using StayGo.Models.ValueObjects;
using StayGo.Integration;
using StayGo.Services;
using StackExchange.Redis;

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

// 1.3. Autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// -----------------
// Redis Configuration (OPCIONAL)
// -----------------
var redisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled", false);

if (redisEnabled)
{
    try
    {
        var redisConfiguration = builder.Configuration.GetValue<string>("Redis:Configuration") ?? "localhost:6379";

        // Registrar ConnectionMultiplexer como singleton
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Program>>();
            try
            {
                var configuration = ConfigurationOptions.Parse(redisConfiguration);
                configuration.AbortOnConnectFail = false;
                configuration.ConnectTimeout = 5000; // 5 segundos timeout
                var connection = ConnectionMultiplexer.Connect(configuration);
                logger.LogInformation("✅ Redis conectado exitosamente en {Configuration}", redisConfiguration);
                return connection;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ No se pudo conectar a Redis. Usando caché en memoria como fallback.");
                throw; // Lanzar para que use el fallback
            }
        });

        // Configurar caché distribuido con Redis
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfiguration;
            options.InstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName") ?? "StayGo:";
        });

        Console.WriteLine("🔵 Redis habilitado - Usando caché distribuido");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error al configurar Redis: {ex.Message}");
        Console.WriteLine("📦 Usando caché en memoria como fallback");
        builder.Services.AddDistributedMemoryCache();
    }
}
else
{
    // Usar caché en memoria si Redis está deshabilitado
    builder.Services.AddDistributedMemoryCache();
    Console.WriteLine("📦 Redis deshabilitado - Usando caché en memoria");
}

// Registrar el servicio de caché personalizado
builder.Services.AddScoped<ICacheService, CacheService>();

// -----------------
// Session (VERY IMPORTANT)
// -----------------
// Sesiones (con Redis si está habilitado, o en memoria)
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
// UnsplashIntegration registration
// -----------------
builder.Services.AddScoped<UnsplashIntegration>();

// -----------------
// MercadoPagoIntegration registration
// -----------------
builder.Services.AddScoped<MercadoPagoIntegration>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<StayGoContext>();
        // Aplicar migraciones pendientes
        context.Database.Migrate();

        // Luego ejecutar el seed
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
