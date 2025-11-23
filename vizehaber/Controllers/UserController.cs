using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vizehaber.Models;
using vizehaber.Repositories;

namespace vizehaber.Controllers
{
    // Bu Controller'a giriş yapmayan kimse giremesin
    [Authorize]
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepository;

        public UserController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
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
            return RedirectToAction("Index");
        }
    }
}