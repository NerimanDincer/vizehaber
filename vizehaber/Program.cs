using AspNetCoreHero.ToastNotification;
using AutoMapper;
using vizehaber.Models;
using vizehaber.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Repository kayýtlarý
// Generic Repository servise tanýtýldý
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

//  Veritabaný baðlantýsý (ekleme)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // giriþ yapýlmadýðýnda yönlenecek sayfa
        options.AccessDeniedPath = "/Account/AccessDenied"; // yetki yoksa yönlenecek sayfa
    });

// Authorization middleware
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//  Middleware sýrasý düzeltildi
app.UseAuthentication();
app.UseAuthorization();

//  Varsayýlan yönlendirme
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();




