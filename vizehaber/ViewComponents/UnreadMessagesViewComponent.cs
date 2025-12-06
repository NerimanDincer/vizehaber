using Microsoft.AspNetCore.Mvc;
using vizehaber.Models;
using vizehaber.Repositories;

namespace vizehaber.ViewComponents
{
    public class UnreadMessagesViewComponent : ViewComponent
    {
        private readonly IRepository<Contact> _contactRepo;

        public UnreadMessagesViewComponent(IRepository<Contact> contactRepo)
        {
            _contactRepo = contactRepo;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Tüm mesajları çek, son 3 tanesini View'a gönder
            // (Gerçek projede "IsRead" alanı olurdu ama şimdilik hepsini sayalım)
            var messages = await _contactRepo.GetAllAsync();
            var recentMessages = messages.OrderByDescending(x => x.CreatedDate).Take(3).ToList();

            // Sayıyı ViewBag ile taşıyalım
            ViewBag.UnreadCount = messages.Count;

            return View(recentMessages);
        }
    }
}