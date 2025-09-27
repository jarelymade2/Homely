using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models;              // ApplicationUser
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // si usas páginas de Identity
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Ruta ABSOLUTA al .db
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "staygo.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

builder.Services.AddDbContext<StayGoContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

// Identity con ApplicationUser + Roles
builder.Services
    .AddDefaultIdentity<ApplicationUser>(opt =>
    {
        opt.Password.RequiredLength = 6;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<StayGoContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Migrar y seed
using (var scope = app.Services.CreateScope())
{
    var sp  = scope.ServiceProvider;
    var ctx = sp.GetRequiredService<StayGoContext>();
    await ctx.Database.MigrateAsync(); // aplica migraciones
    var ds = ctx.Database.GetDbConnection().DataSource;
    Console.WriteLine($"[StayGo] Usando BD: {ds}");
    await Seed.RunAsync(sp);           // tu método de seed
}

app.Run();
