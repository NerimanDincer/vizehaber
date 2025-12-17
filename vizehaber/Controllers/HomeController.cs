using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizehaber.Models;
using vizehaber.Repositories;
using System.Diagnostics;

namespace vizehaber.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Logger eklendi (Error metodunda kullanılıyor)
        private readonly IRepository<News> _newsRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<AppUser> _userRepository;
        private readonly INotyfService _notyf;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context; // Veritabanı bağlantımız

        // YAPICI METOT (CONSTRUCTOR) - EKSİKSİZ
        public HomeController(ILogger<HomeController> logger,
                              IRepository<News> newsRepository,
                              IRepository<Category> categoryRepository,
                              IRepository<AppUser> userRepository,
                              INotyfService notyf,
                              UserManager<AppUser> userManager,
                              AppDbContext context) // 🔥 Context buraya eklendi
        {
            _logger = logger;
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _notyf = notyf;
            _userManager = userManager;
            _context = context; // 🔥 İçeri alındı
        }

        // --- ANASAYFA ---
        public async Task<IActionResult> Index(int? categoryId, string search)
        {
            var newsList = (await _newsRepository.GetAllAsync()).ToList();
            var categories = (await _categoryRepository.GetAllAsync()).ToList();
            var users = (await _userRepository.GetAllAsync()).ToList();

            foreach (var item in newsList)
            {
                item.Category = categories.FirstOrDefault(c => c.Id == item.CategoryId);
                item.AppUser = users.FirstOrDefault(u => u.Id == item.AppUserId);
            }

            if (categoryId.HasValue)
            {
                newsList = newsList.Where(x => x.CategoryId == categoryId.Value).ToList();
            }

            // Anasayfadaki arama (Filtreleme)
            if (!string.IsNullOrEmpty(search))
            {
                newsList = newsList.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(search.ToLower())) ||
                    (x.Content != null && x.Content.ToLower().Contains(search.ToLower()))
                ).ToList();

                if (newsList.Count == 0)
                    _notyf.Warning($"'{search}' ile ilgili haber bulunamadı.");
                else
                    _notyf.Success($"'{search}' için {newsList.Count} sonuç listelendi.");

                ViewBag.SearchTerm = search;
            }

            var finalNews = newsList
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.PublishedDate)
                .ToList();

            return View(finalNews);
        }

        // --- ÜST ARAMA KUTUSU (SEARCH METODU) ---
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            // Eğer kutu boşsa anasayfaya at
            if (string.IsNullOrEmpty(query)) return RedirectToAction("Index");

            ViewData["Query"] = query; // Aranan kelimeyi ekrana yazdırmak için

            // 🔥 DÜZELTİLDİ: Repository yerine Context kullanıyoruz (Hatasız)
            var searchResults = await _context.News
                .Include(x => x.Category)
                .Include(x => x.AppUser)
                .Where(x => x.Title.Contains(query) || x.Content.Contains(query))
                .OrderByDescending(x => x.PublishedDate)
                .ToListAsync();

            return View(searchResults);
        }

        // --- YAZARLAR LİSTESİ ---
        public async Task<IActionResult> Authors()
        {
            var writers = await _userManager.GetUsersInRoleAsync("Writer");
            return View(writers);
        }

        // --- YAZAR PROFİL DETAYI ---
        [HttpGet]
        public async Task<IActionResult> AuthorDetail(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            var allNews = await _newsRepository.GetAllAsync();
            var authorNews = allNews.Where(x => x.AppUserId == id && x.IsActive)
                                    .OrderByDescending(x => x.PublishedDate)
                                    .ToList();

            user.News = authorNews;

            return View(user);
        }

        // --- DİĞER SAYFALAR ---
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        // --- ÖZEL HATA SAYFASI YÖNLENDİRİCİSİ ---
        public IActionResult ErrorPage(int? code)
        {
            if (code == 404)
            {
                ViewData["ErrorMessage"] = "Aradığınız sayfa bulunamadı.";
            }
            else
            {
                ViewData["ErrorMessage"] = "Bir hata oluştu.";
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}