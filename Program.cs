using Microsoft.EntityFrameworkCore;
using StayGo.Data;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// ✅ DbContext con SQLite
builder.Services.AddDbContext<StayGoContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("Sqlite");
    options.UseSqlite(cs);
});

var app = builder.Build();

// (Opcional) Crear la BD si no existe
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StayGoContext>();
    db.Database.EnsureCreated(); // usa migraciones si prefieres: db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
