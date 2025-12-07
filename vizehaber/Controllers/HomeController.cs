using AspNetCoreHero.ToastNotification.Abstractions; // Bildirim Kütüphanesi
using Microsoft.AspNetCore.Mvc;
using vizehaber.Models;
using vizehaber.Repositories;
using System.Diagnostics;

namespace vizehaber.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<News> _newsRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<User> _userRepository;

        // Bildirim servisini ekliyoruz
        private readonly INotyfService _notyf;

        public HomeController(IRepository<News> newsRepository,
                              IRepository<Category> categoryRepository,
                              IRepository<User> userRepository,
                              INotyfService notyf) // Constructor'a ekle
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _notyf = notyf; // İçeri al
        }

        // Hem Kategori ID'si hem de Arama Kelimesi (search) alabilir
        public async Task<IActionResult> Index(int? categoryId, string search)
        {
            // 1. Verileri Çek
            var newsList = await _newsRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();

            // 2. İsimleri Doldur
            foreach (var item in newsList)
            {
                item.Category = categories.FirstOrDefault(c => c.Id == item.CategoryId);
                item.User = users.FirstOrDefault(u => u.Id == item.UserId);
            }

            // 3. FİLTRELEME MANTIĞI

            // Eğer kategoriye tıklandıysa
            if (categoryId.HasValue)
            {
                newsList = newsList.Where(x => x.CategoryId == categoryId.Value).ToList();
                // Opsiyonel: Bildirim vermeye gerek yok, kategoriye girdiği belli.
            }

            // Eğer Arama yapıldıysa
            if (!string.IsNullOrEmpty(search))
            {
                newsList = newsList.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(search.ToLower())) ||
                    (x.Content != null && x.Content.ToLower().Contains(search.ToLower()))
                ).ToList();

                // 🔥 BİLDİRİM BURADA ÇALIŞACAK 🔥
                if (newsList.Count == 0)
                {
                    _notyf.Warning($"'{search}' ile ilgili haber bulunamadı.");
                }
                else
                {
                    _notyf.Success($"'{search}' için {newsList.Count} sonuç listelendi.");
                }

                // Arama kutusunda kelime kalsın diye View'a geri gönderelim
                ViewBag.SearchTerm = search;
            }

            // 4. Sadece AKTİF haberleri göster
            var finalNews = newsList
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.PublishedDate)
                .ToList();

            return View(finalNews);
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

        public async Task<IActionResult> Authors()
        {
            
            var users = await _userRepository.GetAllAsync();
            var authors = users.Where(x => x.Role == "Writer").ToList();

            return View(authors);
        }
    }
}