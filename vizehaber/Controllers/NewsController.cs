using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using vizehaber.Models;
using vizehaber.Repositories;
using System.Security.Claims;

namespace vizehaber.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {
        private readonly IRepository<News> _newsRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NewsController(IRepository<News> newsRepository,
                              IRepository<Category> categoryRepository,
                              IWebHostEnvironment webHostEnvironment)
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var newsList = await _newsRepository.GetAllAsync();
            return View(newsList);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            if (categories == null) categories = new List<Category>();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(News news, IFormFile? file)
        {
            // 1. DÜZELTME: ImageUrl yerine ImagePath kullanıyoruz
            if (file != null && file.Length > 0)
            {
                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "newsPhotos");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                news.ImagePath = "/newsPhotos/" + fileName;
            }
            else
            {
                news.ImagePath = "/sbadmin/img/undraw_posting_photo.svg";
            }

            // 2. DÜZELTME: AppUserId yerine UserId kullanıyoruz
            var userIdString = User.FindFirst("Id")?.Value;

            if (!string.IsNullOrEmpty(userIdString))
            {
                news.UserId = int.Parse(userIdString);
            }

            news.PublishedDate = DateTime.Now;

            ModelState.Remove("Category");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                await _newsRepository.AddAsync(news);
                return RedirectToAction("Index");
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(news);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _newsRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}