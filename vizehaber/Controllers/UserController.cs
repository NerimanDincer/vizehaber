using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vizehaber.Models;
using vizehaber.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace vizehaber.Controllers
{
    // Bu Controller'a giriş yapmayan kimse giremesin
    [Authorize]
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly INotyfService _notyf;

        public UserController(IRepository<User> userRepository, INotyfService notyf) // EKLENDİ
        {
            _userRepository = userRepository;
            _notyf = notyf;
        }

        // 1. KULLANICI LİSTESİ (Sadece Admin görebilsin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        // 2. PROFİLİM SAYFASI (Herkes kendi profilini görsün)
        public async Task<IActionResult> Profile()
        {
            // Cookie'den giriş yapan kullanıcının ID'sini al
            var userIdString = User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdString);

            var user = await _userRepository.GetByIdAsync(userId);
            return View(user);
        }

        // 3. KULLANICI SİLME (Sadece Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userRepository.DeleteAsync(id);
            _notyf.Warning("Kullanıcı sistemden silindi."); // BİLDİRİM
            return RedirectToAction("Index");
        }

        // PROFİL DÜZENLEME SAYFASI (GET)
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userIdString = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var user = await _userRepository.GetByIdAsync(int.Parse(userIdString));
            return View(user);
        }

        // PROFİL GÜNCELLEME İŞLEMİ (POST)
        [HttpPost]
        public async Task<IActionResult> EditProfile(User model, IFormFile? file)
        {
            // Veritabanındaki orijinal kullanıcıyı çek
            var user = await _userRepository.GetByIdAsync(model.Id);
            if (user == null) return NotFound();

            // Sadece izin verilen alanları güncelle
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.UserName;
            user.Biography = model.Biography;
            user.UpdatedDate = DateTime.Now;

            // Fotoğraf güncelleme
            if (file != null && file.Length > 0)
            {
                // ... (NewsController'daki resim yükleme kodunun aynısı buraya) ...
                // Kısaca:
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/userPhotos");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string path = Path.Combine(folder, fileName);
                using (var stream = new FileStream(path, FileMode.Create)) { await file.CopyToAsync(stream); }

                user.PhotoPath = "/userPhotos/" + fileName;
            }

            // Şifre değiştirme istenirse buraya eklenebilir ama şimdilik kalsın.

            await _userRepository.UpdateAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.UserName ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("Id", user.Id.ToString()),
                new Claim("PhotoPath", user.PhotoPath ?? "/sbadmin/img/undraw_profile.svg")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(7) };

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);


            return RedirectToAction("Profile");
        }
    }
}