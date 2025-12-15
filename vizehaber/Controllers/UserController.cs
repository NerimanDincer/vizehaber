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
        // --- KENDİ PROFİLİNİ DÜZENLE (GET) ---
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = new UserUpdateViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Biography = user.Biography,
                PhotoUrl = user.PhotoUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserUpdateViewModel model, IFormFile? file)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // 1. Temel Bilgileri Güncelle
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Biography = model.Biography;

            // 2. Resim Yükleme (Aynı kalıyor)
            if (file != null && file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);
                var newImageName = Guid.NewGuid() + extension;
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/newsPhotos");

                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var location = Path.Combine(folderPath, newImageName);
                using (var stream = new FileStream(location, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                user.PhotoUrl = "/newsPhotos/" + newImageName;
            }

            // 3. Şifre Değiştirme (GÜNCELLENDİ) 🛠️
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                // A) Mevcut şifre girilmemişse uyar
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    _notyf.Error("Şifre değiştirmek için mevcut şifrenizi girmelisiniz.");
                    return View(model);
                }

                // B) 🔥 YENİ KONTROL: Eski ve Yeni şifre aynı mı?
                if (model.CurrentPassword == model.NewPassword)
                {
                    _notyf.Warning("Yeni şifreniz, mevcut şifrenizle aynı olamaz!");
                    return View(model);
                }

                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (!passwordChangeResult.Succeeded)
                {
                    foreach (var error in passwordChangeResult.Errors)
                    {
                        // C) 🔥 TÜRKÇELEŞTİRME: Hata kodu "PasswordMismatch" ise biz mesajı değiştiriyoruz
                        if (error.Code == "PasswordMismatch")
                        {
                            _notyf.Error("Mevcut şifrenizi yanlış girdiniz!");
                        }
                        else
                        {
                            // Diğer hatalar (örn: şifre çok kısa vs.) yine İngilizce gelebilir
                            // Onları da tek tek yakalayabilirsin ama şimdilik böyle kalsın.
                            _notyf.Error(error.Description);
                        }
                    }
                    return View(model);
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _notyf.Success("Profiliniz başarıyla güncellendi.");
                return RedirectToAction("Profile");
            }

            _notyf.Error("Güncelleme sırasında hata oluştu.");
            return View(model);
        }
    }
}