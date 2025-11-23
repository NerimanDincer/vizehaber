using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vizehaber.Models;
using vizehaber.Repositories; // Repository klasörünü eklemeyi unutma
using vizehaber.ViewModels;

namespace vizehaber.Controllers
{
    public class AccountController : Controller
    {
        // 1. DbContext yerine Repository kullanıyoruz (Kriter Şartı)
        private readonly IRepository<User> _userRepository;

        public AccountController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            // Zaten giriş yapmışsa anasayfaya at
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 2. Repository üzerinden kullanıcıyı buluyoruz
            // Not: FindAsync List döndürdüğü için FirstOrDefault ile tek kayıt alıyoruz.
            var users = await _userRepository.FindAsync(u =>
                (u.UserName == model.UserNameOrEmail || u.Email == model.UserNameOrEmail)
                && u.Password == model.Password
                && u.IsActive);

            var user = users.FirstOrDefault();

            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre yanlış!");
                return View(model);
            }

            // 3. COOKIE OLUŞTURMA (Hocanın "Cookie bazlı oturum" kriteri)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName), // Ad Soyad
                new Claim(ClaimTypes.NameIdentifier, user.UserName), // Kullanıcı adı
                new Claim(ClaimTypes.Role, user.Role), // Rol (Admin/User vs)
                new Claim("Id", user.Id.ToString()), // ID'yi de saklayalım lazım olur
                new Claim("PhotoPath", user.PhotoPath ?? "/userPhotos/default.png") // Profil fotosu
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Beni hatırla mantığı (tarayıcı kapansa da gitmez)
                ExpiresUtc = DateTime.UtcNow.AddDays(7) // 7 gün kalsın
            };

            // Session yerine SignInAsync kullanıyoruz
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kullanıcı var mı kontrolü (Repository ile)
            var existingUsers = await _userRepository.FindAsync(x => x.Email == model.Email || x.UserName == model.UserName);
            if (existingUsers.Any())
            {
                ModelState.AddModelError("", "Bu e-posta veya kullanıcı adı zaten kayıtlı.");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Email = model.Email, // View modeline Email eklemen iyi olur
                Password = model.Password,
                Role = model.Role ?? "User", // Rol seçilmediyse varsayılan User olsun
                IsActive = true,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                PhotoPath = "/userPhotos/default.png" // Varsayılan foto
            };

            // 4. Repository ile Ekleme
            await _userRepository.AddAsync(user);

            return RedirectToAction("Login");
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            // Session.Clear yerine SignOutAsync
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View(); // Erişim engellendi sayfası
        }
    }
}