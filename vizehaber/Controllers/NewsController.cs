using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using vizehaber.Models;
using vizehaber.Repositories;

namespace vizehaber.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {
        private readonly IRepository<News> _newsRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Comment> _commentRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotyfService _notyf;

        public NewsController(IRepository<News> newsRepository,
                              IRepository<Category> categoryRepository,
                              IRepository<Comment> commentRepository,
                              IRepository<User> userRepository,
                              IWebHostEnvironment webHostEnvironment,
                              INotyfService notyf)
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _webHostEnvironment = webHostEnvironment;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            var newsList = await _newsRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();

            foreach (var item in newsList)
            {
                item.Category = categories.FirstOrDefault(c => c.Id == item.CategoryId);
                item.User = users.FirstOrDefault(u => u.Id == item.UserId);
            }

            // Admin panelinde hepsi görünsün (Aktif/Pasif fark etmez)
            return View(newsList.OrderByDescending(x => x.PublishedDate).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            if (categories == null) categories = new List<Category>();
            ViewBag.Categories = new SelectList(categories.Where(c => c.IsActive), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(News news, IFormFile? file)
        {
            if (file != null && file.Length > 0)
            {
                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "newsPhotos");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                news.ImagePath = "/newsPhotos/" + fileName;
            }
            else
            {
                news.ImagePath = "/sbadmin/img/undraw_posting_photo.svg";
            }

            var userIdString = User.FindFirst("Id")?.Value;
            if (!string.IsNullOrEmpty(userIdString)) news.UserId = int.Parse(userIdString);

            news.PublishedDate = DateTime.Now;
            news.IsActive = true;

            ModelState.Remove("Category");
            ModelState.Remove("User");
            ModelState.Remove("Comments");

            if (ModelState.IsValid)
            {
                await _newsRepository.AddAsync(news);
                _notyf.Success("Haber başarıyla eklendi!");
                return RedirectToAction("Index");
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories.Where(c => c.IsActive), "Id", "Name");
            return View(news);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _newsRepository.GetByIdAsync(id);
            if (news == null) return NotFound();

            int currentUserId = int.Parse(User.FindFirst("Id")?.Value ?? "0");

            // Eğer kullanıcı ADMIN DEĞİLSE ve Haberin Yazarı da KENDİSİ DEĞİLSE -> At dışarı!
            if (!User.IsInRole("Admin") && news.UserId != currentUserId)
            {
                _notyf.Error("Bu haberi düzenleme yetkiniz yok! Sadece kendi haberlerinizi düzenleyebilirsiniz.");
                return RedirectToAction("Index");
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories.Where(c => c.IsActive), "Id", "Name", news.CategoryId);

            return View(news);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(News news, IFormFile? file)
        {
            var existingNews = await _newsRepository.GetByIdAsync(news.Id);
            if (existingNews == null) return NotFound();

            // 🔥 GÜVENLİK KONTROLÜ (POST tarafında da şart!) 🔥
            int currentUserId = int.Parse(User.FindFirst("Id")?.Value ?? "0");

            if (!User.IsInRole("Admin") && existingNews.UserId != currentUserId)
            {
                _notyf.Error("Yetkisiz işlem girişimi!");
                return RedirectToAction("Index");
            }

            // Resim güncelleme (Aynen kalıyor)
            if (file != null && file.Length > 0)
            {
                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "newsPhotos");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                existingNews.ImagePath = "/newsPhotos/" + fileName;
            }

            // Alanları güncelle
            existingNews.Title = news.Title;
            existingNews.Content = news.Content;
            existingNews.CategoryId = news.CategoryId;
            existingNews.UpdatedDate = DateTime.Now;

            await _newsRepository.UpdateAsync(existingNews);
            _notyf.Success("Haber güncellendi!");

            return RedirectToAction("Index");
        }

        // --- SİLME (HARD DELETE) ---
        public async Task<IActionResult> Delete(int id)
        {
            // Önce haberi bulalım ki sahibini kontrol edebilelim
            var news = await _newsRepository.GetByIdAsync(id);
            if (news == null) return NotFound();

            // 🔥 GÜVENLİK KONTROLÜ 🔥
            int currentUserId = int.Parse(User.FindFirst("Id")?.Value ?? "0");

            if (!User.IsInRole("Admin") && news.UserId != currentUserId)
            {
                _notyf.Error("Başkasına ait bir haberi silemezsiniz!");
                return RedirectToAction("Index");
            }

            await _newsRepository.DeleteAsync(id);
            _notyf.Error("Haber kalıcı olarak silindi.");

            return RedirectToAction("Index");
        }

        // --- DURUM DEĞİŞTİR (ASIKIYA ALMA) ---
        [Authorize(Roles = "Admin,Writer")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var news = await _newsRepository.GetByIdAsync(id);
            if (news == null) return NotFound();

            // 🔥 GÜVENLİK KONTROLÜ 🔥
            int currentUserId = int.Parse(User.FindFirst("Id")?.Value ?? "0");

            if (!User.IsInRole("Admin") && news.UserId != currentUserId)
            {
                _notyf.Error("Bu haberin durumunu değiştirme yetkiniz yok.");
                return RedirectToAction("Index");
            }

            news.IsActive = !news.IsActive;
            await _newsRepository.UpdateAsync(news);

            if (news.IsActive) _notyf.Success("Haber yayına alındı.");
            else _notyf.Warning("Haber askıya alındı.");

            return RedirectToAction("Index");
        }

        // --- AJAX İÇİN YORUM METODU ---
        [HttpPost]
        public async Task<IActionResult> AddComment(int newsId, string text)
        {
            var userIdString = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                return Json(new { success = false, message = "Lütfen giriş yapın." });
            }

            var userFullName = User.Identity.Name;

            var comment = new Comment
            {
                NewsId = newsId,
                Text = text,
                UserId = int.Parse(userIdString),
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            await _commentRepository.AddAsync(comment);

            return Json(new
            {
                success = true,
                userName = userFullName,
                date = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                text = text
            });
        }

        // --- HABER DETAY ---
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var news = await _newsRepository.GetByIdAsync(id);
            if (news == null) return NotFound();

            if (news.CategoryId != 0)
                news.Category = await _categoryRepository.GetByIdAsync(news.CategoryId);

            if (news.UserId != 0)
                news.User = await _userRepository.GetByIdAsync(news.UserId);

            var allComments = await _commentRepository.GetAllAsync();
            var newsComments = allComments.Where(x => x.NewsId == id).OrderByDescending(x => x.CreatedDate).ToList();

            var users = await _userRepository.GetAllAsync();
            foreach (var comment in newsComments)
            {
                comment.User = users.FirstOrDefault(u => u.Id == comment.UserId);
            }

            news.Comments = newsComments;
            return View(news);
        }
    }
}