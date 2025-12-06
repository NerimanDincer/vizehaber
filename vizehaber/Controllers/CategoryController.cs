using AspNetCoreHero.ToastNotification.Abstractions; // Kütüphane
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vizehaber.Models;
using vizehaber.Repositories;

namespace vizehaber.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly INotyfService _notyf; // Servisi tanımla

        public CategoryController(IRepository<Category> categoryRepository, INotyfService notyf)
        {
            _categoryRepository = categoryRepository;
            _notyf = notyf; // İçeri al
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Add() => View();

        [HttpPost]
        public async Task<IActionResult> Add(Category category)
        {
            if (ModelState.IsValid)
            {
                category.CreatedDate = DateTime.Now;
                category.UpdatedDate = DateTime.Now;
                category.IsActive = true;

                await _categoryRepository.AddAsync(category);
                _notyf.Success("Kategori başarıyla eklendi!"); // 🔥 BİLDİRİM
                return RedirectToAction("Index");
            }
            _notyf.Error("Lütfen bilgileri kontrol edin.");
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Category category)
        {
            if (ModelState.IsValid)
            {
                category.UpdatedDate = DateTime.Now;
                await _categoryRepository.UpdateAsync(category);
                _notyf.Success("Kategori güncellendi!"); // 🔥 BİLDİRİM
                return RedirectToAction("Index");
            }
            _notyf.Error("Güncelleme başarısız.");
            return View(category);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _categoryRepository.DeleteAsync(id);
            _notyf.Warning("Kategori silindi."); // 🔥 BİLDİRİM
            return RedirectToAction("Index");
        }
    }
}