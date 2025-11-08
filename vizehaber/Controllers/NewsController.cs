using Microsoft.AspNetCore.Mvc;

namespace vizehaber.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
