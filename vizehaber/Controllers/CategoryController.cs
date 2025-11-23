using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vizehaber.Models;
using vizehaber.Repositories;

namespace vizehaber.Controllers
{
    public class CategoryController : Controller
    {
        // ARTIK GENERIC REPOSITORY KULLANIYORUZ
        private readonly IRepository<Category> _categoryRepository;

        public CategoryController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Tüm kategorileri getir
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        // Kategori Ekleme Sayfası (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Kategori Ekleme İşlemi (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.AddAsync(category);
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // Düzenleme Sayfası (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Düzenleme İşlemi (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.UpdateAsync(category);
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // Silme Sayfası (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Silme İşlemi (POST - Onay)
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}