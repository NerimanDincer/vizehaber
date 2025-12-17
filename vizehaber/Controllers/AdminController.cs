using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using vizehaber.Models;
using vizehaber.Repositories;

namespace vizehaber.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IRepository<News> _newsRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<AppUser> _userRepository; // User -> AppUser oldu
        private readonly IRepository<Comment> _commentRepository;

        public AdminController(IRepository<News> newsRepository,
                               IRepository<Category> categoryRepository,
                               IRepository<AppUser> userRepository, // User -> AppUser oldu
                               IRepository<Comment> commentRepository)
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _commentRepository = commentRepository;
        }

        public async Task<IActionResult> Index()
        {
            var news = (await _newsRepository.GetAllAsync()).ToList();
            var categories = (await _categoryRepository.GetAllAsync()).ToList();
            var users = (await _userRepository.GetAllAsync()).ToList();
            var comments = (await _commentRepository.GetAllAsync()).ToList();

            ViewBag.NewsCount = news.Count;
            ViewBag.CategoryCount = categories.Count;
            ViewBag.UserCount = users.Count;
            ViewBag.CommentCount = comments.Count;

            return View();
        }

        public async Task<IActionResult> GetReport()
        {
            //verileri çekme kısmı
            var news = (await _newsRepository.GetAllAsync()).ToList();
            var categories = (await _categoryRepository.GetAllAsync()).ToList();
            var users = (await _userRepository.GetAllAsync()).ToList();
            var comments = (await _commentRepository.GetAllAsync()).ToList();

            var builder = new StringBuilder();

            // --- BAŞLIK ---
            
            builder.AppendLine($"Rapor Tarihi:;{DateTime.Now:dd.MM.yyyy HH:mm}");
            builder.AppendLine("");

            // --- İSTATİSTİKLER ---
            builder.AppendLine("GENEL ISTATISTIKLER");
            builder.AppendLine("Baslik;Adet");
            builder.AppendLine($"Toplam Haber Sayisi;{news.Count}");
            builder.AppendLine($"Toplam Kategori Sayisi;{categories.Count}");
            builder.AppendLine($"Toplam Kullanici Sayisi;{users.Count}");
            builder.AppendLine($"Toplam Yorum Sayisi;{comments.Count}");
            builder.AppendLine("");

            // --- KULLANICI LİSTESİ ---
            builder.AppendLine("SON UYE OLAN KULLANICILAR");
            
            builder.AppendLine("Ad Soyad;Email;Unvan (Uzmanlik);Kayit Tarihi");

            foreach (var user in users.OrderByDescending(x => x.CreatedDate).Take(20)) // Son 20 kişiyi getir
            {
                // Unvan boşsa "-" yazsın
                string unvan = string.IsNullOrEmpty(user.Specialization) ? "-" : user.Specialization;

                // CSV satırını oluşturuyoruz
                builder.AppendLine($"{user.FullName};{user.Email};{unvan};{user.CreatedDate:dd.MM.yyyy}");
            }

            var content = builder.ToString();
            var buffer = Encoding.UTF8.GetBytes(content);
            var bom = Encoding.UTF8.GetPreamble();

            var result = new byte[bom.Length + buffer.Length];
            Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
            Buffer.BlockCopy(buffer, 0, result, bom.Length, buffer.Length);

            return File(result, "text/csv", $"SistemRaporu_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }
    }
}