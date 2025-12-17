using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizehaber.Models;
using vizehaber.ViewModels; // ViewModel klasörünü unutma

namespace vizehaber.Controllers
{
    [Authorize] // Herkes profilini görebilsin
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

        // --- 1. KULLANICI LİSTESİ ---
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string search)
        {
            // 1. Sorguyu Hazırla
            var usersQuery = _userManager.Users.AsQueryable();

            // 2. Arama varsa FİLTRELE
            if (!string.IsNullOrEmpty(search))
            {
                usersQuery = usersQuery.Where(u =>
                    u.UserName.Contains(search) ||
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search));

                ViewData["SearchTerm"] = search;
            }

            // 3. Kullanıcıları Çek
            var users = await usersQuery.ToListAsync();

            // AppUser listesini UserRoleViewModel listesine çeviriyoruz
            var userRolesViewModel = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var thisViewModel = new UserRoleViewModel();
                thisViewModel.Id = user.Id;
                thisViewModel.Email = user.Email;
                thisViewModel.UserName = user.UserName;
                thisViewModel.FullName = user.FullName;
                thisViewModel.PhotoUrl = user.PhotoUrl;

                // HATAYI ÇÖZEN KISIM:
                // Veritabanından rolleri çekiyoruz
                var roles = await _userManager.GetRolesAsync(user);

                // Listeden ilk rolü alıp 'Role' değişkenine atıyoruz. Yoksa 'Kullanıcı' yazıyoruz.
                // Artık "List<string>" hatası vermeyecek çünkü string'e çevirdik.
                thisViewModel.Role = roles.FirstOrDefault() ?? "Kullanıcı";

                userRolesViewModel.Add(thisViewModel);
            }

            return View(userRolesViewModel);
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
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new UserRoleViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                PhotoUrl = user.PhotoUrl,
                Specialization = user.Specialization,
                SelectedRole = userRoles.FirstOrDefault()
            };

            // --- BURAYI GÜNCELLEDİK ---

            // 1. Veritabanındaki mevcut rolleri çek (Liste olarak al)
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            // 2. Eğer listede 'User' yoksa ELLE EKLE (Ki seçebilelim)
            if (!allRoles.Contains("User"))
            {
                allRoles.Add("User");
            }

            // 3. Eğer listede 'Writer' veya 'Admin' eksikse onları da garanti olsun diye ekle
            if (!allRoles.Contains("Writer")) allRoles.Add("Writer");
            if (!allRoles.Contains("Admin")) allRoles.Add("Admin");

            ViewBag.Roles = allRoles;

            return View(model);
        }

        // --- 4. KULLANICI DÜZENLEME (POST) (HATA 1 ÇÖZÜMÜ) ---
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken] // Güvenlik için
        public async Task<IActionResult> EditUser(UserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            if (!await _roleManager.RoleExistsAsync(model.SelectedRole))
            {
                await _roleManager.CreateAsync(new AppRole { Name = model.SelectedRole });
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            await _userManager.AddToRoleAsync(user, model.SelectedRole);

            user.Specialization = model.Specialization;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _notyf.Success($"{user.FullName} başarıyla güncellendi. ✅");
                return RedirectToAction("Index"); // Listeye geri dön
            }
            else
            {
                // Hata varsa ekrana bas
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> WriterList()
        {
            var writers = await _userManager.GetUsersInRoleAsync("Writer");

            return View(writers);
        }

        // --- KULLANICIYI ASKIYA AL / ENGELİ KALDIR ---
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleBan(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Eğer şu an engelli ise -> Engelini Kaldır
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                user.LockoutEnd = null; // Kilidi aç
                await _userManager.UpdateAsync(user);
                _notyf.Success($"{user.FullName} kullanıcısının engeli kaldırıldı. ✅");
            }
            else
            {
                // Eğer engelli değilse -> 30 Günlük (veya sonsuz) Engel Koy
                user.LockoutEnd = DateTime.Now.AddDays(30); // İstersen AddYears(99) yapabilirsin
                await _userManager.UpdateAsync(user);
                _notyf.Warning($"{user.FullName} 30 günlüğüne askıya alındı. ⛔");
            }

            // İşlem bitince Yazar Listesine geri dön
            return RedirectToAction("WriterList");
        }

        // --- KULLANICIYI SİL ---
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Kullanıcıyı sil
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                _notyf.Success($"{user.FullName} sistemden tamamen silindi. 🗑️");
            }
            else
            {
                _notyf.Error("Silme işlemi başarısız oldu. Kullanıcının haberleri veya yorumları olabilir.");
            }

            return RedirectToAction("WriterList");
        }
    }
}