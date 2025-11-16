using Microsoft.AspNetCore.Mvc;
using vizehaber.ViewModels;
using vizehaber.Models; // User modelimiz için
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace vizehaber.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users
                        .FirstOrDefault(u => u.UserName == model.UserNameOrEmail && u.Password == model.Password && u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre yanlış!");
                return View(model);
            }

            // Session ile login
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kullanıcı kaydı
            var user = new User
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Password = model.Password,
                Role = model.Role,
                IsActive = true,
                Created = DateTime.Now,
                Updated = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
