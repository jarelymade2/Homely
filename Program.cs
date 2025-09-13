using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StayGo.Data;
using StayGo.Models.ValueObjects;
using FluentValidation;
using FluentValidation.AspNetCore;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddDbContext<StayGoContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("sqlite")));

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

// ← ejecuta el seed
await Seed.RunAsync(app.Services);

app.Run();
