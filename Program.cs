using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ✅ Ruta ABSOLUTA a Data\staygo.db (misma para migraciones y runtime)
var dbPath = Path.Combine(builder.Environment.ContentRootPath,"staygo.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

builder.Services.AddDbContext<StayGoContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));
// Si prefieres leer de appsettings y normalizar por si es relativa:
// var cs = builder.Configuration.GetConnectionString("Sqlite"); // ojo: "Sqlite" con S mayúscula
// if (!Path.IsPathRooted(new SqliteConnectionStringBuilder(cs).DataSource)) {
//     var abs = Path.Combine(builder.Environment.ContentRootPath, new SqliteConnectionStringBuilder(cs).DataSource);
//     cs = $"Data Source={abs}";
// }
// builder.Services.AddDbContext<StayGoContext>(opt => opt.UseSqlite(cs));

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(opt =>
    {
        opt.Password.RequiredLength = 6;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
    })
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

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ Ejecuta seed en scope y registra qué archivo .db se usa
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var ctx = sp.GetRequiredService<StayGoContext>();
    var ds = ctx.Database.GetDbConnection().DataSource;
    Console.WriteLine($"[StayGo] Usando BD: {ds}");
    await Seed.RunAsync(sp);
}

app.Run();
