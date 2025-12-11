using Microsoft.AspNetCore.Mvc;
using System.Linq;
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
            // 1. Veriyi çekip hemen Listeye çeviriyoruz (.ToList() ekledik)
            // Böylece Count hatası vermez ve veriyi hafızaya alırız.
            var allMessages = (await _contactRepo.GetAllAsync()).ToList();

            // 2. Toplam Okunmamış Sayısını ViewBag'e atıyoruz (Rozet için)
            ViewBag.UnreadCount = allMessages.Count;

            // 3. Sadece son 3 tanesini alıp View'a gönderiyoruz (Liste için)
            var recentMessages = allMessages.OrderByDescending(x => x.CreatedDate).Take(3).ToList();

            return View(recentMessages);
        }
    }
}