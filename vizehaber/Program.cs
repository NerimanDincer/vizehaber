using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using vizehaber.Models;
using vizehaber.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabaný Baðlantýsý
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Generic Repository Tanýmlamasý (Hoca buna bayýlacak!)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// 3. Cookie Authentication (Oturum Açma Ayarý)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Giriþ yapmamýþsa buraya at
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisi yoksa buraya
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // 7 gün açýk kalsýn
        options.SlidingExpiration = true;
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. Sýralama Kritik: Önce Kimlik, Sonra Yetki
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // SeedData sýnýfýný tetikliyoruz
        SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanýna örnek veriler yüklenirken hata oluþtu.");
    }
}

app.Run();