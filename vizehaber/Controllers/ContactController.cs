using Microsoft.AspNetCore.Mvc;
using vizehaber.Models;

namespace vizehaber.Controllers
{
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ContactController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
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
        public IActionResult Index(Contact model, IFormFile? Photo)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Fotoğraf yükleme
            if (Photo != null && Photo.Length > 0)
            {
                string uploads = Path.Combine(_env.WebRootPath, "contactPhotos");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                string fileName = Guid.NewGuid() + Path.GetExtension(Photo.FileName);
                string filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Photo.CopyTo(stream);
                }

                model.PhotoPath = "/contactPhotos/" + fileName;
            }

            model.Created = DateTime.Now;
            model.Updated = DateTime.Now;
            model.IsActive = true;

            _context.Contacts.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "İhbarınız başarıyla gönderildi!";
            return RedirectToAction("Index");
        }
    }
}

