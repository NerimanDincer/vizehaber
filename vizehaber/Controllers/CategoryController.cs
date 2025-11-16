using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using vizehaber.Models;
using vizehaber.Repositories;
using vizehaber.ViewModels;

namespace vizehaber.Controllers
{
   // [Authorize(Roles = "Admin")] // Sadece admin kullanıcılar erişebilir. Normal kullanıcı veya yazar girmeye çalışırsa engellenir.

    public class CategoryController : Controller
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly NewsRepository _newsRepository;
        private readonly INotyfService _notyf;
        private readonly IMapper _mapper; //Category-CategoryModel dönüşümleri için AutoMapper kullanılıyor.

        public CategoryController(CategoryRepository categoryRepository, NewsRepository newsRepository, INotyfService notyf, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _newsRepository = newsRepository;
            _notyf = notyf;
            _mapper = mapper;
        }

        // Kategori Listesi
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var categoryModels = _mapper.Map<List<CategoryModel>>(categories);
            return View(categoryModels);
        }

        // Kategori Ekleme Sayfası
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(CategoryModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var category = _mapper.Map<Category>(model);
            category.Created = DateTime.Now;
            category.Updated = DateTime.Now;
            await _categoryRepository.AddAsync(category);

            _notyf.Success("Kategori eklendi!");
            return RedirectToAction("Index");
        }

        // Kategori Güncelleme
        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            var categoryModel = _mapper.Map<CategoryModel>(category);
            return View(categoryModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(CategoryModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var category = await _categoryRepository.GetByIdAsync(model.Id);
            category.Name = model.Name;
            category.IsActive = model.IsActive;
            category.Updated = DateTime.Now;
            await _categoryRepository.UpdateAsync(category);

            _notyf.Success("Kategori güncellendi!");
            return RedirectToAction("Index");
        }

        // Kategori Silme
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            var categoryModel = _mapper.Map<CategoryModel>(category);
            return View(categoryModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(CategoryModel model)
        {
            // Eğer kategoride haber varsa silme
            var news = await _newsRepository.GetAllAsync();
            if (news.Count(n => n.CategoryId == model.Id) > 0)
            {
                _notyf.Error("Bu kategoride haberler mevcut, silinemez!");
                return RedirectToAction("Index");
            }

            await _categoryRepository.DeleteAsync(model.Id);
            _notyf.Success("Kategori silindi!");
            return RedirectToAction("Index");
        }
    }
}

