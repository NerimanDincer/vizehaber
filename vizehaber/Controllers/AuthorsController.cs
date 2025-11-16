using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizehaber.Models;
using System.Threading.Tasks;

namespace vizehaber.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly AppDbContext _context;

        public AuthorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Authors/Index
        public async Task<IActionResult> Index()
        {
            var authors = await _context.Authors
                .Where(a => a.IsActive) // Sadece aktif yazarlar
                .ToListAsync();
            return View(authors);
        }

        // İstersen detay sayfası için
        public async Task<IActionResult> Details(int id)
        {
            var author = await _context.Authors
                .Include(a => a.News) // Yazarın haberlerini de çek
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                return NotFound();

            return View(author);
        }
    }
}
