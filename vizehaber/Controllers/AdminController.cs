using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text; // CSV oluşturmak için gerekli
using vizehaber.Models;
using vizehaber.Repositories;

namespace vizehaber.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IRepository<News> _newsRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Comment> _commentRepository;

        public AdminController(IRepository<News> newsRepository,
                               IRepository<Category> categoryRepository,
                               IRepository<User> userRepository,
                               IRepository<Comment> commentRepository)
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _commentRepository = commentRepository;
        }

        public async Task<IActionResult> Index()
        {
            // İstatistikleri veritabanından çekiyoruz
            var news = await _newsRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();
            var comments = await _commentRepository.GetAllAsync();

            // ViewBag ile View tarafına taşıyoruz
            ViewBag.NewsCount = news.Count;
            ViewBag.CategoryCount = categories.Count;
            ViewBag.UserCount = users.Count;
            ViewBag.CommentCount = comments.Count;

            return View();
        }

        // 🔥 YENİ EKLENEN ÖZELLİK: RAPOR İNDİRME 🔥
        public async Task<IActionResult> GetReport()
        {
            var news = await _newsRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();
            var comments = await _commentRepository.GetAllAsync();

            // CSV (Excel) Formatında Rapor Hazırlama
            var builder = new StringBuilder();

            // Başlık Satırı
            builder.AppendLine("Rapor Tarihi:," + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
            builder.AppendLine(""); // Boş satır

            // İstatistikler
            builder.AppendLine("İSTATİSTİKLER");
            builder.AppendLine("Kategori,Adet");
            builder.AppendLine($"Toplam Haber Sayısı,{news.Count}");
            builder.AppendLine($"Toplam Kategori Sayısı,{categories.Count}");
            builder.AppendLine($"Toplam Kullanıcı Sayısı,{users.Count}");
            builder.AppendLine($"Toplam Yorum Sayısı,{comments.Count}");
            builder.AppendLine("");

            // Detaylı Kullanıcı Listesi
            builder.AppendLine("SON ÜYE OLAN KULLANICILAR");
            builder.AppendLine("Ad Soyad,Email,Rol,Kayıt Tarihi");

            foreach (var user in users.OrderByDescending(x => x.CreatedDate).Take(10)) // Son 10 kişi
            {
                builder.AppendLine($"{user.FullName},{user.Email},{user.Role},{user.CreatedDate}");
            }

            // Dosya haline getirip indir
            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "sistem_raporu.csv");
        }
    }
}