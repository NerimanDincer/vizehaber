using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vizehaber.Models;
using vizehaber.Repositories;

namespace vizehaber.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Patron Girebilir
    public class CommentController : Controller
    {
        private readonly IRepository<Comment> _commentRepository;
        private readonly IRepository<News> _newsRepository;
        private readonly IRepository<AppUser> _userRepository;
        private readonly INotyfService _notyf;

        public CommentController(IRepository<Comment> commentRepository,
                                 IRepository<News> newsRepository,
                                 IRepository<AppUser> userRepository,
                                 INotyfService notyf)
        {
            _commentRepository = commentRepository;
            _newsRepository = newsRepository;
            _userRepository = userRepository;
            _notyf = notyf;
        }

        public async Task<IActionResult> Index()
        {
            var comments = await _commentRepository.GetAllAsync();
            var news = await _newsRepository.GetAllAsync();
            var users = await _userRepository.GetAllAsync();

            // İlişkileri Doldur (Hangi habere, kim yazmış?)
            foreach (var item in comments)
            {
                item.News = news.FirstOrDefault(n => n.Id == item.NewsId);
                item.AppUser = users.FirstOrDefault(u => u.Id == item.AppUserId);
            }

            // En yeniden eskiye sırala
            return View(comments.OrderByDescending(x => x.CreatedDate).ToList());
        }

        // Yorumu Sil (Hard Delete)
        public async Task<IActionResult> Delete(int id)
        {
            await _commentRepository.DeleteAsync(id);
            _notyf.Warning("Yorum kalıcı olarak silindi.");
            return RedirectToAction("Index");
        }

        // Yorumu Gizle/Göster (Soft Delete)
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return NotFound();

            comment.IsActive = !comment.IsActive; // Durumu tersine çevir
            await _commentRepository.UpdateAsync(comment);

            _notyf.Success(comment.IsActive ? "Yorum yayına alındı." : "Yorum gizlendi.");
            return RedirectToAction("Index");
        }
    }
}