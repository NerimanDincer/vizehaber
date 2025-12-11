using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity; // 🔥 BU EKSİK OLABİLİR
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ToListAsync için gerekli
using vizehaber.Models;
using vizehaber.Repositories;
using System.Diagnostics;

namespace vizehaber.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<News> _newsRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<AppUser> _userRepository;
        private readonly INotyfService _notyf;

        // 🔥 1. UserManager'ı burada tanımlıyoruz
        private readonly UserManager<AppUser> _userManager;

        // 🔥 2. Constructor'da (Yapıcı Metot) içeri alıyoruz
        public HomeController(IRepository<News> newsRepository,
                              IRepository<Category> categoryRepository,
                              IRepository<AppUser> userRepository,
                              INotyfService notyf,
                              UserManager<AppUser> userManager) // <-- Buraya ekledik
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _notyf = notyf;
            _userManager = userManager; // <-- Eşleştirdik
        }

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

        // --- YAZARLAR SAYFASI (DÜZELTİLDİ) ---
        public async Task<IActionResult> Authors()
        {
            // 🔥 ARTIK HATA VERMEZ: Sadece "Writer" rolündeki kullanıcıları getir
            var writers = await _userManager.GetUsersInRoleAsync("Writer");

            return View(writers);
        }

        // --- YAZAR DETAY SAYFASI ---
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

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}