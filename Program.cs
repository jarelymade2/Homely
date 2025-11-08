using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums;
using StayGo.Models.ValueObjects;
using StayGo.Integration;
using StayGo.Services;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using StayGo.Services.AI;

var builder = WebApplication.CreateBuilder(args);

// -----------------
// CONFIGURACIÓN GLOBAL
// -----------------
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// -----------------
// Connection string
// -----------------
var connectionString = configuration.GetConnectionString("StayGoContext")
    ?? throw new InvalidOperationException("Connection string 'StayGoContext' not found.");

builder.Services.AddDbContext<StayGoContext>(options =>
    options.UseSqlite(connectionString));

// -----------------
// Identity (with Roles)
// -----------------
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

// -----------------
// Google reCAPTCHA
// -----------------
builder.Services.Configure<GoogleReCaptchaSettings>(
    configuration.GetSection("GoogleReCaptcha")); // ✅ Carga desde appsettings.json

// -----------------
// Redis Configuration (opcional)
// -----------------
var redisEnabled = configuration.GetValue<bool>("Redis:Enabled", false);

if (redisEnabled)
{
    try
    {
        var redisConfiguration = configuration.GetValue<string>("Redis:Configuration") ?? "localhost:6379";

        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Program>>();
            try
            {
                var redisOptions = ConfigurationOptions.Parse(redisConfiguration);
                redisOptions.AbortOnConnectFail = false;
                redisOptions.ConnectTimeout = 5000;
                var connection = ConnectionMultiplexer.Connect(redisOptions);
                logger.LogInformation("✅ Redis conectado exitosamente en {Configuration}", redisConfiguration);
                return connection;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ No se pudo conectar a Redis. Usando caché en memoria como fallback.");
                throw;
            }
        });

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfiguration;
            options.InstanceName = configuration.GetValue<string>("Redis:InstanceName") ?? "StayGo:";
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
    builder.Services.AddDistributedMemoryCache();
    Console.WriteLine("📦 Redis deshabilitado - Usando caché en memoria");
}

builder.Services.AddScoped<ICacheService, CacheService>();

// -----------------
// Session
// -----------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// -----------------
// Integraciones externas
// -----------------
builder.Services.AddHttpClient<OpenWeatherIntegration>();
builder.Services.AddScoped<OpenWeatherIntegration>();
builder.Services.AddScoped<UnsplashIntegration>();
builder.Services.AddScoped<MercadoPagoIntegration>();

builder.Services.AddScoped<IChatAiService, OllamaChatService>();



var app = builder.Build();

// -----------------
// SEEDING DATABASE
// -----------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<StayGoContext>();
        context.Database.Migrate();
        await Seed.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error al inicializar la base de datos.");
    }
}

// -----------------
// PIPELINE
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
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// -----------------
// RUTAS
// -----------------
// RUTA PARA ÁREAS (Admin)
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


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

// -----------------
// CLASE PARA RECAPTCHA
// -----------------
public class GoogleReCaptchaSettings
{
    public string SiteKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}
