using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using vizehaber.Models;
using vizehaber.ViewModels; // ViewModel için bu satır şart!

namespace vizehaber.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly INotyfService _notyf;

        // Constructor (Yapıcı Metot)
        public AccountController(UserManager<AppUser> userManager,
                                 SignInManager<AppUser> signInManager,
                                 INotyfService notyf)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _notyf = notyf;
        }

        // --- GİRİŞ YAP (LOGIN) ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // --- GİRİŞ YAP (LOGIN) - POST ---
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kullanıcıyı bul
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                // Şifreyi kontrol et
                // 3. parametre (model.RememberMe): Beni Hatırla özelliği
                // 4. parametre (false): Şifreyi çok yanlış girerse kilitleme (şimdilik kapalı)
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    _notyf.Success($"Hoş geldiniz, {user.FullName}");

                    // Eğer admin ise Admin paneline, değilse Anasayfaya gitsin
                    // (Şimdilik direkt anasayfaya yönlendiriyoruz)
                    return RedirectToAction("Index", "Home");
                }
            }

            _notyf.Error("E-posta veya şifre hatalı!");
            return View(model);
        }

        // --- KAYIT OL (REGISTER) ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // 1. Model geçerli mi?
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. ViewModel'den AppUser'a çevir
            var appUser = new AppUser
            {
                UserName = model.Email, // Kullanıcı adı E-posta olsun
                Email = model.Email,
                FullName = model.FullName,
                PhotoUrl = "/sbadmin/img/undraw_profile.svg", // Varsayılan resim
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            // 3. Kaydet
            var result = await _userManager.CreateAsync(appUser, model.Password);

            if (result.Succeeded)
            {
                _notyf.Success("Kayıt başarılı! Giriş yapabilirsiniz.");
                return RedirectToAction("Login");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    _notyf.Error(error.Description);
                }
            }

            // Hata varsa formu tekrar göster
            return View(model);
        }

        // --- ÇIKIŞ YAP (LOGOUT) ---
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _notyf.Information("Çıkış yapıldı.");
            return RedirectToAction("Index", "Home");
        }

        // --- ERİŞİM ENGELLENDİ ---
        public IActionResult AccessDenied()
        {
            _notyf.Warning("Bu sayfaya erişim yetkiniz yok!");
            return RedirectToAction("Index", "Home");
        }
    }
}