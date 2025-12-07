using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vizehaber.Models;
using vizehaber.Repositories;
using vizehaber.Services;
using vizehaber.ViewModels;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace vizehaber.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly INotyfService _notyf;

        public AccountController(IRepository<User> userRepository, INotyfService notyf)
        {
            _userRepository = userRepository;
            _notyf = notyf;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string hashedPassword = GeneralService.HashPassword(model.Password);
            var users = await _userRepository.FindAsync(x => x.Email == model.UserNameOrEmail || x.UserName == model.UserNameOrEmail);
            var user = users.FirstOrDefault(x => x.Password == hashedPassword);

            if (user != null)
            {
                if (!user.IsActive)
                {
                    _notyf.Error("Hesabınız askıya alınmıştır. Giriş yapamazsınız.");
                    return View(model);
                }
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName ?? ""),
                    new Claim(ClaimTypes.NameIdentifier, user.UserName ?? ""),
                    new Claim(ClaimTypes.Role, user.Role ?? "User"),
                    new Claim("Id", user.Id.ToString()),
                    new Claim("PhotoPath", user.PhotoPath ?? "/sbadmin/img/undraw_profile.svg")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _notyf.Success($"Hoşgeldin {user.FullName}!", 3);
                return RedirectToAction("Index", "Home");
            }

            _notyf.Error("Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // _notyf.Information("Oturum kapatıldı.");
            return RedirectToAction("Login");
        }

        // --- İŞTE BU EKSİKTİ, EKLENDİ ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        // --------------------------------

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var existingUsers = await _userRepository.FindAsync(x => x.Email == model.Email || x.UserName == model.UserName);
                if (existingUsers.Any())
                {
                    _notyf.Warning("Bu kullanıcı zaten kayıtlı.");
                    return View(model);
                }

                var newUser = new User
                {
                    FullName = model.FullName,
                    UserName = model.UserName,
                    Email = model.Email,
                    Password = GeneralService.HashPassword(model.Password),
                    Role = "User",
                    PhotoPath = "/sbadmin/img/undraw_profile.svg",
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                await _userRepository.AddAsync(newUser);
                _notyf.Success("Kayıt başarılı! Giriş yapabilirsiniz.");
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Hata: " + ex.Message);
                _notyf.Error("Kayıt başarısız oldu.");
                return View(model);
            }
        }

        public IActionResult AccessDenied()
        {
            _notyf.Error("Bu sayfaya erişim yetkiniz yok!");
            return RedirectToAction("Index", "Home");
        }
    }
}