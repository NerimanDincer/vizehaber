using AspNetCoreHero.ToastNotification;
using Microsoft.AspNetCore.Identity; // 🔥 BU LAZIM
using Microsoft.EntityFrameworkCore;
using vizehaber.Models;
using vizehaber.Repositories; // Data klasöründeyse namespace'i ona göre düzelt

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVİSLERİN EKLENMESİ ---

builder.Services.AddControllersWithViews();

// --- 2. VERİTABANI BAĞLANTISI ---
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// --- 3. 🔥 IDENTITY AYARLARI (Kritik Nokta) ---
builder.Services.AddIdentity<AppUser, AppRole>(opt =>
{
    // Şifre Zorluk Ayarları
    opt.Password.RequireDigit = false;           // Rakam zorunlu değil
    opt.Password.RequireLowercase = false;       // Küçük harf zorunlu değil
    opt.Password.RequireUppercase = false;       // Büyük harf zorunlu değil
    opt.Password.RequireNonAlphanumeric = false; // Özel karakter (@, #) zorunlu değil
    opt.Password.RequiredLength = 3;             // En az 3 karakter olsun yeter

    // Kullanıcı Ayarları
    opt.User.RequireUniqueEmail = true;          // Aynı mailden 2 tane olmasın
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// --- 4. COOKIE (ÇEREZ) AYARLARI ---
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";        // Giriş yapmamışsa buraya at
    opt.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisi yoksa buraya at
    opt.ExpireTimeSpan = TimeSpan.FromDays(7);      // 7 Gün hatırla
    opt.SlidingExpiration = true;                   // Siteye girdikçe süreyi uzat
});

// --- 5. REPOSITORY YAPISI ---
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// --- 6. BİLDİRİM SERVİSİ (Notyf) ---
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 10;
    config.IsDismissable = true;
    config.Position = NotyfPosition.BottomRight;
});

builder.Services.AddSignalR();

var app = builder.Build();

// --- HTTP REQUEST PIPELINE ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/Home/ErrorPage", "?code={0}"); //404 hatalarını yakalama kısmı
app.UseStaticFiles();

app.UseRouting();

// Önce Kimlik (Authentication), Sonra Yetki (Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<vizehaber.Hubs.GeneralHub>("/general-hub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Eski SeedData artık çalışmaz çünkü User tablosu değişti.
// O yüzden şimdilik burayı yorum satırına alıyorum.
/*
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
        logger.LogError(ex, "Veritabanı başlatılırken hata oluştu.");
    }
}
*/

app.Run();