using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizehaber.Models;
using vizehaber.ViewModels; // ViewModel klasörünü unutma

namespace vizehaber.Controllers
{
    [Authorize] // Herkes profilini görebilsin ama...
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager; // 🔥 Rol Yönetimi Eklendi
        private readonly INotyfService _notyf;

        public UserController(UserManager<AppUser> userManager,
                              RoleManager<AppRole> roleManager, // Constructor'a eklendi
                              INotyfService notyf)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _notyf = notyf;
        }

        // --- 1. KULLANICI LİSTESİ (ROLLERİYLE BERABER) ---
        [Authorize(Roles = "Admin")] // Sadece Admin görebilir
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userListViewModel = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                // Her kullanıcının rolünü çekiyoruz
                var roles = await _userManager.GetRolesAsync(user);

                userListViewModel.Add(new UserRoleViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    SelectedRole = roles.FirstOrDefault() ?? "Rol Yok" // İlk rolü al, yoksa "Rol Yok" yaz
                    // IsActive bilgisini ViewModel'e eklememiştik, gerekirse ekleriz 
                    // ama şimdilik Id üzerinden işlem yapıyoruz.
                });
            }

            return View(userListViewModel);
        }

        // --- 2. PROFİL SAYFASI (HATA 3 ÇÖZÜMÜ) ---
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Giriş yapan kullanıcıyı bul
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            return View(user); // Views/User/Profile.cshtml sayfasına gider
        }

        // --- 3. KULLANICI DÜZENLEME (GET) ---
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new UserRoleViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                SelectedRole = userRoles.FirstOrDefault() ?? "User"
            };
            return View(model);
        }

        // --- 4. KULLANICI DÜZENLEME (POST) (HATA 1 ÇÖZÜMÜ) ---
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditUser(UserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // 🔥 HATA 1 ÇÖZÜMÜ: Rol veritabanında yoksa OLUŞTUR!
            if (!await _roleManager.RoleExistsAsync(model.SelectedRole))
            {
                await _roleManager.CreateAsync(new AppRole { Name = model.SelectedRole });
            }

            // Mevcut rolleri sil
            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            // Yeni rolü ata
            await _userManager.AddToRoleAsync(user, model.SelectedRole);

            _notyf.Success($"{user.FullName} yetkisi '{model.SelectedRole}' oldu.");
            return RedirectToAction("Index");
        }

        // --- 5. KULLANICIYI AKTİF/PASİF YAP ---
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (User.Identity.Name == user.UserName)
            {
                _notyf.Warning("Kendinizi askıya alamazsınız!");
                return RedirectToAction("Index");
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            if (user.IsActive) _notyf.Success("Kilit açıldı.");
            else _notyf.Warning("Kullanıcı askıya alındı.");

            return RedirectToAction("Index");
        }

        // --- 6. SİLME ---
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (User.Identity.Name == user.UserName)
            {
                _notyf.Error("Kendinizi silemezsiniz!");
                return RedirectToAction("Index");
            }

            await _userManager.DeleteAsync(user);
            _notyf.Success("Kullanıcı silindi.");
            return RedirectToAction("Index");
        }
    }
}