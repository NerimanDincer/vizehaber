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
            // Verileri çekip hemen Listeye çeviriyoruz (.ToList())
            // Böylece .Count özelliği hata vermez.
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
            // Burada da .ToList() ekledik
            var news = (await _newsRepository.GetAllAsync()).ToList();
            var categories = (await _categoryRepository.GetAllAsync()).ToList();
            var users = (await _userRepository.GetAllAsync()).ToList();
            var comments = (await _commentRepository.GetAllAsync()).ToList();

            var builder = new StringBuilder();

            builder.AppendLine("Rapor Tarihi:," + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
            builder.AppendLine("");

            builder.AppendLine("ISTATISTIKLER");
            builder.AppendLine("Kategori,Adet");
            builder.AppendLine($"Toplam Haber Sayisi,{news.Count}");
            builder.AppendLine($"Toplam Kategori Sayisi,{categories.Count}");
            builder.AppendLine($"Toplam Kullanici Sayisi,{users.Count}");
            builder.AppendLine($"Toplam Yorum Sayisi,{comments.Count}");
            builder.AppendLine("");

            builder.AppendLine("SON UYE OLAN KULLANICILAR");
            builder.AppendLine("Ad Soyad,Email,Kayit Tarihi"); // Rolü sildim çünkü AppUser'da direkt Role yok

            foreach (var user in users.OrderByDescending(x => x.CreatedDate).Take(10))
            {
                // AppUser özelliklerini yazdırıyoruz
                builder.AppendLine($"{user.FullName},{user.Email},{user.CreatedDate}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "sistem_raporu.csv");
        }
    }
}