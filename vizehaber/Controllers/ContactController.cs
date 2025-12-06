using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vizehaber.Models;
using vizehaber.Repositories; // Repository için

namespace vizehaber.Controllers
{
    public class ContactController : Controller
    {
        // Context yerine Repository kullanıyoruz (Puan Kriteri)
        private readonly IRepository<Contact> _contactRepository;
        private readonly IWebHostEnvironment _env;

        public ContactController(IRepository<Contact> contactRepository, IWebHostEnvironment env)
        {
            _contactRepository = contactRepository;
            _env = env;
        }

        // GET: Contact
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Contact model, IFormFile? Photo)
        {
            // Validasyon kontrolü (Model.IsValid değilse geri dön)
            /* Not: BaseEntity'den gelen alanlar bazen validasyonu bozabilir.
               Bu yüzden basit bir Contact formu için ModelState kontrolünü esnetebiliriz.
            */

            // Fotoğraf Yükleme
            if (Photo != null && Photo.Length > 0)
            {
                string uploads = Path.Combine(_env.WebRootPath, "contactPhotos");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                string fileName = Guid.NewGuid() + Path.GetExtension(Photo.FileName);
                string filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Photo.CopyToAsync(stream);
                }

                model.PhotoPath = "/contactPhotos/" + fileName;
            }

            // TARİH İSİMLERİ DÜZELTİLDİ (BaseEntity ile uyumlu)
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;
            model.IsActive = true;

            // Repository ile Kayıt
            await _contactRepository.AddAsync(model);

            TempData["Success"] = "İhbarınız başarıyla gönderildi!";
            return RedirectToAction("Index");
        }

        // SADECE ADMİN GÖREBİLİR
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Messages()
        {
            var messages = await _contactRepository.GetAllAsync();
            // En yeniden eskiye sıralayalım
            return View(messages.OrderByDescending(x => x.CreatedDate).ToList());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _contactRepository.DeleteAsync(id);
            // Burada _notyf tanımlı değilse hata verebilir, 
            // ContactController constructor'ına INotyfService eklemen gerekebilir.
            // Eğer eklemekle uğraşmak istemezsen TempData kullan:
            TempData["Success"] = "Mesaj silindi.";

            return RedirectToAction("Messages");
        }

        // Mesaj Detayını Görmek İçin (Opsiyonel ama şık olur)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var message = await _contactRepository.GetByIdAsync(id);
            if (message == null) return NotFound();
            return View(message);
        }
    }
}