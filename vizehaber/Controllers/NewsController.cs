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
        private readonly IRepository<User> _userRepository; // User Repo lazım oldu
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotyfService _notyf;

        public NewsController(IRepository<News> newsRepository,
                              IRepository<Category> categoryRepository,
                              IRepository<Comment> commentRepository,
                              IRepository<User> userRepository, // Eklendi
                              IWebHostEnvironment webHostEnvironment,
                              INotyfService notyf)
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _commentRepository = commentRepository;
            _userRepository = userRepository; // Eklendi
            _webHostEnvironment = webHostEnvironment;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index(string search)
        {
            // 1. Tüm verileri çekiyoruz
            var newsList = await _newsRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();

            // 2. Haberlerin içini dolduruyoruz (Include Mantığı - Manuel)
            foreach (var item in newsList)
            {
                item.Category = categories.FirstOrDefault(c => c.Id == item.CategoryId);
                item.User = users.FirstOrDefault(u => u.Id == item.UserId);
            }
            // ARAMA FİLTRESİ
            if (!string.IsNullOrEmpty(search))
            {
                newsList = newsList.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(search.ToLower())) ||
                    (x.Content != null && x.Content.ToLower().Contains(search.ToLower()))
                ).ToList();

                ViewBag.SearchTerm = search;

                if (newsList.Count == 0)
                {
                    _notyf.Warning($"'{search}' ile ilgili hiçbir haber bulunamadı.");
                }
                else
                {
                    _notyf.Information($"'{search}' için {newsList.Count} sonuç bulundu.");
                }
            }

            // Admin panelinde hepsi görünsün (Sıralama ile)
            return View(newsList.OrderByDescending(x => x.PublishedDate).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            // Sadece aktif kategoriler listelensin
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
            news.IsActive = true; // Eklenen haber aktif olsun

            ModelState.Remove("Category");
            ModelState.Remove("User");
            ModelState.Remove("Comments");

            if (ModelState.IsValid)
            {
                await _newsRepository.AddAsync(news);
                _notyf.Success("Haber eklendi!");
                return RedirectToAction("Index");
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories.Where(c => c.IsActive), "Id", "Name");
            return View(news);
        }

        // --- DÜZENLEME (EDIT) KISMI ---
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _newsRepository.GetByIdAsync(id);
            if (news == null) return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories.Where(c => c.IsActive), "Id", "Name", news.CategoryId);

            return View(news);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(News news, IFormFile? file)
        {
            // Eski veriyi çekip bazı bilgileri korumamız lazım (Tarih, Yazar vs.)
            var existingNews = await _newsRepository.GetByIdAsync(news.Id);
            if (existingNews == null) return NotFound();

            // Yeni resim varsa güncelle, yoksa eskisi kalsın
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

            // Değişen alanları güncelle
            existingNews.Title = news.Title;
            existingNews.Content = news.Content;
            existingNews.CategoryId = news.CategoryId;
            existingNews.UpdatedDate = DateTime.Now;

            await _newsRepository.UpdateAsync(existingNews);
            _notyf.Success("Haber güncellendi!");

            return RedirectToAction("Index");
        }

        // --- SİLME (SOFT DELETE - ASKIYA ALMA) ---
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _newsRepository.GetByIdAsync(id);
            if (news != null)
            {
                news.IsActive = false; // Silmiyoruz, Pasif yapıyoruz
                await _newsRepository.UpdateAsync(news);
                _notyf.Warning("Haber çöp kutusuna taşındı (Askıya alındı).");
            }
            return RedirectToAction("Index");
        }

        // --- AJAX İÇİN YORUM METODU (DÜZELTİLDİ) ---
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

        //  HABER DETAY
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            // 1. Haberi Getir
            var news = await _newsRepository.GetByIdAsync(id);
            if (news == null) return NotFound();

            // 2. KATEGORİ BİLGİSİNİ DOLDUR (Manuel Include)
            if (news.CategoryId != 0)
            {
                news.Category = await _categoryRepository.GetByIdAsync(news.CategoryId);
            }

            // 3. YAZAR BİLGİSİNİ DOLDUR (Manuel Include)
            if (news.UserId != 0)
            {
                news.User = await _userRepository.GetByIdAsync(news.UserId);
            }

            // 4. YORUMLARI GETİR (Zaten yapmıştık ama garanti olsun)
            var allComments = await _commentRepository.GetAllAsync();
            var newsComments = allComments.Where(x => x.NewsId == id).OrderByDescending(x => x.CreatedDate).ToList();

            // Yorumların yazarlarını da dolduralım ki "Kullanıcı" yazmasın, isim yazsın
            var users = await _userRepository.GetAllAsync();
            foreach (var comment in newsComments)
            {
                comment.User = users.FirstOrDefault(u => u.Id == comment.UserId);
            }

            news.Comments = newsComments;

            return View(news);
        }

        [Authorize(Roles = "Admin,Writer")] // Yazar da kendi haberini kaldırabilsin
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var news = await _newsRepository.GetByIdAsync(id);
            if (news == null) return NotFound();

            // Durumu tersine çevir (True ise False, False ise True yap)
            news.IsActive = !news.IsActive;

            await _newsRepository.UpdateAsync(news);

            _notyf.Success(news.IsActive ? "Haber yayına alındı." : "Haber yayından kaldırıldı.");

            return RedirectToAction("Index");
        }


    }
}