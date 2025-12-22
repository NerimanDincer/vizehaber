using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Identity için gerekli
using vizehaber.Models;
using vizehaber.Repositories;
using Microsoft.AspNetCore.SignalR;
using vizehaber.Hubs;

namespace vizehaber.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {
        private readonly IRepository<News> _newsRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Comment> _commentRepository;
        private readonly IRepository<AppUser> _userRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotyfService _notyf;

        private readonly AppDbContext _context;
        private readonly IHubContext<GeneralHub> _hubContext;

        public NewsController(IRepository<News> newsRepository,
                              IRepository<Category> categoryRepository,
                              IRepository<Comment> commentRepository,
                              IRepository<AppUser> userRepository,
                              IWebHostEnvironment webHostEnvironment,
                              INotyfService notyf,
                              AppDbContext context,
                              IHubContext<GeneralHub> hubContext)
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _webHostEnvironment = webHostEnvironment;
            _notyf = notyf;
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var newsList = await _newsRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();

            foreach (var item in newsList)
            {
                item.Category = categories.FirstOrDefault(c => c.Id == item.CategoryId);
                // ID eşleşmesi (String olduğu için == yeterli)
                item.AppUser = users.FirstOrDefault(u => u.Id == item.AppUserId);
            }

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

            // --- ID ALMA KISMI (String olarak alıyoruz) ---
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId)) news.AppUserId = userId;

            news.PublishedDate = DateTime.Now;
            news.IsActive = true;

            // Model Validation Hatalarını Temizle
            ModelState.Remove("Category");
            ModelState.Remove("AppUser");
            ModelState.Remove("Comments");
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                await _newsRepository.AddAsync(news);
                _notyf.Success("Haber başarıyla eklendi!");
                await _hubContext.Clients.All.SendAsync("ReceiveNewsNotification", news.Title, news.Id);
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

            // Giriş yapan kullanıcının ID'si (String)
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Yetki Kontrolü
            if (!User.IsInRole("Admin") && news.AppUserId != currentUserId)
            {
                _notyf.Error("Bu haberi düzenleme yetkiniz yok!");
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

            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!User.IsInRole("Admin") && existingNews.AppUserId != currentUserId)
            {
                _notyf.Error("Yetkisiz işlem!");
                return RedirectToAction("Index");
            }

            // --- RESİM GÜNCELLEME KISMI (Senin kodun) ---
            if (file != null && file.Length > 0)
            {
                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "newsPhotos");
                // Klasör yoksa oluştur (Garanti olsun)
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                existingNews.ImagePath = "/newsPhotos/" + fileName;
            }


            // 1. Validasyon Hatalarını Temizle
            ModelState.Remove("Category");
            ModelState.Remove("AppUser");
            ModelState.Remove("Comments");
            ModelState.Remove("AppUserId");

            // 2. Metin Alanlarını Güncelle (Formdan gelen yeni verileri eskisinin üzerine yaz)
            existingNews.Title = news.Title;
            existingNews.Content = news.Content;
            existingNews.CategoryId = news.CategoryId;
            existingNews.UpdatedDate = DateTime.Now;

            // 3. Veritabanına Kaydet
            await _newsRepository.UpdateAsync(existingNews);

            _notyf.Success("Haber başarıyla güncellendi!");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var news = await _newsRepository.GetByIdAsync(id);
            if (news == null) return NotFound();

            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!User.IsInRole("Admin") && news.AppUserId != currentUserId)
            {
                _notyf.Error("Başkasına ait bir haberi silemezsiniz!");
                return RedirectToAction("Index");
            }

            await _newsRepository.DeleteAsync(id);
            _notyf.Error("Haber silindi.");
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Writer")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var news = await _newsRepository.GetByIdAsync(id);
            if (news == null) return NotFound();

            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!User.IsInRole("Admin") && news.AppUserId != currentUserId)
            {
                _notyf.Error("Yetkiniz yok.");
                return RedirectToAction("Index");
            }

            news.IsActive = !news.IsActive;
            await _newsRepository.UpdateAsync(news);

            if (news.IsActive) _notyf.Success("Yayına alındı.");
            else _notyf.Warning("Askıya alındı.");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int newsId, string text)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Lütfen giriş yapın." });
            }

            string userName = User.Identity.Name;

            var comment = new Comment
            {
                NewsId = newsId,
                Text = text,
                AppUserId = userId,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            await _commentRepository.AddAsync(comment);

            string date = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            // SignalR ile herkese bildir
            await _hubContext.Clients.All.SendAsync("ReceiveComment", newsId, userName, text, date);

            // GÜNCELLEME: Sayfayı yenileme, JSON dön ki Ajax mutlu olsun
            return Json(new { success = true });
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            // 🔥 _newsRepository YERİNE _context KULLANIYORUZ
            var news = await _context.News
                .Include(x => x.Category)
                .Include(x => x.AppUser)        // Haberin Yazarı
                .Include(x => x.Comments)       // Yorumlar
                .ThenInclude(c => c.AppUser)    // Yorumu Yapan Kullanıcı (Kritik nokta!)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (news == null) return NotFound();

            // Okunma sayısını artır
            news.ViewCount++;
            _context.News.Update(news);
            await _context.SaveChangesAsync();

            return View(news);
        }
    }
}