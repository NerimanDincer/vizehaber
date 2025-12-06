using AspNetCoreHero.ToastNotification; // 1. HATA BURADAYDI (Eksikti)
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using vizehaber.Models;
using vizehaber.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --- SERVÝSLERÝN EKLENMESÝ ---

builder.Services.AddControllersWithViews();

// 2. Veritabaný Baðlantýsý
// (Hocanýn kodunda "sqlCon" yazýyor, sende "DefaultConnection" olabilir. appsettings.json dosyaný kontrol et)
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 3. Repository Yapýsý (Bizimki Generic, daha pratik)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// 4. Bildirim Servisi (Notyf Ayarlarý - Hocanýnkiyle ayný)
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 10;
    config.IsDismissable = true;
    config.Position = NotyfPosition.BottomRight; // Sað altta çýksýn
});

// 5. Giriþ ve Çerez Ayarlarý (Cookie Auth)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.Cookie.Name = "VizeHaberAuth"; // Çerez adý
        opt.ExpireTimeSpan = TimeSpan.FromDays(7); // 7 gün kalsýn
        opt.LoginPath = "/Account/Login";   // Giriþ sayfasý yolu
        opt.AccessDeniedPath = "/Account/AccessDenied"; // Yetki yok sayfasý
        opt.LogoutPath = "/Account/Logout";
        opt.SlidingExpiration = true; // Kullanýcý aktifse süreyi uzat
    });

var app = builder.Build();

// --- HTTP REQUEST PIPELINE (Uygulama Ayarlarý) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 6. Önce Kimlik, Sonra Yetki (Sýralama Önemli)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 7. SeedData (Veritabaný baþlatýcý)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabaný baþlatýlýrken hata oluþtu.");
    }
}

app.Run();