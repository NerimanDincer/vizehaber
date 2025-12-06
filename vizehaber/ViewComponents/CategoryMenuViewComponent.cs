using Microsoft.AspNetCore.Mvc;
using vizehaber.Repositories;
using vizehaber.Models;

namespace vizehaber.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly IRepository<Category> _categoryRepository;

        public CategoryMenuViewComponent(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Sadece Aktif kategorileri getirip menüye basacağız
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories.Where(x => x.IsActive).ToList());
        }
    }
}