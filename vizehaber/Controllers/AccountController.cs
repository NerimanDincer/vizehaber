using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vizehaber.Models;
using vizehaber.Repositories;
using vizehaber.ViewModels; // LoginViewModel buradaysa

namespace vizehaber.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository<User> _userRepository;

        public AccountController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
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

            // Kullanıcıyı bul
            var users = await _userRepository.FindAsync(x => x.Email == model.UserNameOrEmail || x.UserName == model.UserNameOrEmail);
            var user = users.FirstOrDefault(x => x.Password == model.Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName ?? ""),
                    new Claim(ClaimTypes.NameIdentifier, user.UserName ?? ""),
                    new Claim(ClaimTypes.Role, user.Role ?? "User"),
                    new Claim("Id", user.Id.ToString()),
                    new Claim("PhotoPath", user.PhotoPath ?? "/sbadmin/img/undraw_profile.svg")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Hatalı giriş!";
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var newUser = new User
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Email = model.Email,
                Password = model.Password,
                Role = "User",
                PhotoPath = "/sbadmin/img/undraw_profile.svg",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            await _userRepository.AddAsync(newUser);
            return RedirectToAction("Login");
        }
    }
}