using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

using StayGo.Data;
using StayGo.Models;
using StayGo.Integration;
using StayGo.Services;
<<<<<<< HEAD
using StayGo.Services.AI;
using StayGo.Services.Email;

// Alias para evitar ambigüedades si existen clases con el mismo nombre en otros espacios
using SgEmailOptions = StayGo.Services.Email.EmailSenderOptions;
using SgEmailSender  = StayGo.Services.Email.EmailSender;
=======
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using StayGo.Services.AI;
using OfficeOpenXml;

// >>> ML Integration
using StayGo.Services.ML; // Asegúrate que el namespace coincida con la carpeta donde está MLRecommendationService
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952

var builder = WebApplication.CreateBuilder(args);

// -----------------
<<<<<<< HEAD
// MVC / Razor
// -----------------
=======
// CONFIGURACIÓN GLOBAL
// -----------------
var configuration = builder.Configuration;






// Add services to the container.
// Add services to the container.
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// -----------------
// Connection string
// -----------------
<<<<<<< HEAD
var connectionString = builder.Configuration.GetConnectionString("StayGoContext")
=======
var connectionString = configuration.GetConnectionString("StayGoContext")
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
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
<<<<<<< HEAD
// Google OAuth (login con Google) — validación para evitar nulls
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
        o.ClientId = googleClientId!;
        o.ClientSecret = googleClientSecret!;
        // o.Scope.Add("email");
        // o.Scope.Add("profile");
    });

// -----------------
// Redis (opcional)
=======
// Google reCAPTCHA
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
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
<<<<<<< HEAD
                var configuration = ConfigurationOptions.Parse(redisConfiguration);
                configuration.AbortOnConnectFail = false;
                configuration.ConnectTimeout = 5000; // 5s
                var connection = ConnectionMultiplexer.Connect(configuration);
                logger.LogInformation("✅ Redis conectado en {Configuration}", redisConfiguration);
=======
                var redisOptions = ConfigurationOptions.Parse(redisConfiguration);
                redisOptions.AbortOnConnectFail = false;
                redisOptions.ConnectTimeout = 5000;
                var connection = ConnectionMultiplexer.Connect(redisOptions);

                logger.LogInformation("✅ Redis conectado exitosamente en {Configuration}", redisConfiguration);
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
                return connection;
            }
            catch (Exception ex)
            {
<<<<<<< HEAD
                logger.LogWarning(ex, "⚠️ No se pudo conectar a Redis. Usando caché en memoria.");
=======
                logger.LogWarning(ex, "⚠️ No se pudo conectar a Redis. Usando caché en memoria como fallback.");
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
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

<<<<<<< HEAD
// -----------------
// Cache service propio
// -----------------
=======
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
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
<<<<<<< HEAD
// Integraciones HTTP
=======
// Integraciones externas
// (junto lo de feat/ml-recommendations + develop)
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
// -----------------
builder.Services.AddHttpClient<OpenWeatherIntegration>();
builder.Services.AddScoped<OpenWeatherIntegration>();
builder.Services.AddScoped<UnsplashIntegration>();
builder.Services.AddScoped<MercadoPagoIntegration>();
<<<<<<< HEAD

// -----------------
// Chatbot (Ollama) - Servicio de IA
// -----------------
builder.Services.AddScoped<IChatAiService, OllamaChatService>();

// -----------------
// Email (SendGrid) - Forgot/Reset password
// -----------------
builder.Services.Configure<SgEmailOptions>(builder.Configuration.GetSection("SendGrid"));
builder.Services.AddTransient<IEmailSender, SgEmailSender>();

// (Opcional) Log de verificación rápida
Console.WriteLine($"SendGrid configured: {!string.IsNullOrWhiteSpace(builder.Configuration["SendGrid:ApiKey"])}");
=======
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952

// Servicio de chat (rama develop)
builder.Services.AddScoped<IChatAiService, OllamaChatService>();

// Servicio de recomendaciones ML (rama feat/ml-recommendations)
builder.Services.AddScoped<MLRecommendationService>();

// EPPlus licencia (rama develop)
ExcelPackage.License.SetNonCommercialPersonal("Jarel");

var app = builder.Build();

// -----------------
<<<<<<< HEAD
// Migraciones + Seed
=======
// SEEDING DATABASE
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952
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

        // Entrenamiento del modelo (rama feat/ml-recommendations)
        var ml = services.GetRequiredService<MLRecommendationService>();
        ml.TrainModel();
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
<<<<<<< HEAD

app.UseAuthentication();
app.UseAuthorization();

// Controladores por atributo (API del chatbot, etc.)
app.MapControllers();
=======
app.UseAuthentication();
app.UseAuthorization();

// =========================================================
// 4. RUTAS
// =========================================================
>>>>>>> 2833e7d27de18370600e88d248c29b43aa20f952

// Áreas (Admin)
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
