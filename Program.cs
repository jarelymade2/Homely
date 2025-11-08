using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;
using StayGo.Models.Enums;
using StayGo.Models.ValueObjects;
using StayGo.Integration;
using StayGo.Services;
using StackExchange.Redis;
using StayGo.Services.AI; // Chatbot (IChatAiService, OllamaChatService)
using Microsoft.AspNetCore.Identity.UI.Services;
using StayGo.Services.Email;

var builder = WebApplication.CreateBuilder(args);

// -----------------
// MVC / Razor
// -----------------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("SendGrid"));
builder.Services.AddTransient<IEmailSender, EmailSender>();
// -----------------
// Connection string
// -----------------
var connectionString = builder.Configuration.GetConnectionString("StayGoContext")
    ?? throw new InvalidOperationException("Connection string 'StayGoContext' not found.");

builder.Services.AddDbContext<StayGoContext>(options =>
    options.UseSqlite(connectionString));

// -----------------
// Identity (con Roles)
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
// Google OAuth (login con Google) — validación para evitar warnings
// -----------------
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
{
    throw new InvalidOperationException(
        "Faltan claves de Google. Define Authentication:Google:ClientId y Authentication:Google:ClientSecret " +
        "en user-secrets o variables de entorno."
    );
}

builder.Services
    .AddAuthentication()
    .AddGoogle(o =>
    {
        o.ClientId = googleClientId;
        o.ClientSecret = googleClientSecret;
        // o.Scope.Add("email");
        // o.Scope.Add("profile");
    });

// -----------------
// Redis (opcional)
// -----------------
var redisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled", false);

if (redisEnabled)
{
    try
    {
        var redisConfiguration = builder.Configuration.GetValue<string>("Redis:Configuration") ?? "localhost:6379";

        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Program>>();
            try
            {
                var configuration = ConfigurationOptions.Parse(redisConfiguration);
                configuration.AbortOnConnectFail = false;
                configuration.ConnectTimeout = 5000; // 5s
                var connection = ConnectionMultiplexer.Connect(configuration);
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
    builder.Services.AddDistributedMemoryCache();
    Console.WriteLine("📦 Redis deshabilitado - Usando caché en memoria");
}

// -----------------
// Cache service propio
// -----------------
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
// Integraciones HTTP
// -----------------
builder.Services.AddHttpClient<OpenWeatherIntegration>();
builder.Services.AddScoped<OpenWeatherIntegration>();
builder.Services.AddScoped<UnsplashIntegration>();
builder.Services.AddScoped<MercadoPagoIntegration>();

// -----------------
// Chatbot (Ollama) - Servicio de IA
// -----------------
builder.Services.AddScoped<IChatAiService, OllamaChatService>();

var app = builder.Build();

// -----------------
// Migraciones + Seed
// -----------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<StayGoContext>();
        context.Database.Migrate();

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

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// 👇 Necesario para controladores con rutas por atributo (p.ej. /api/ChatApi)
app.MapControllers();

// Áreas (Admin)
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
