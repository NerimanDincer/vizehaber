using Microsoft.AspNetCore.Mvc;

namespace vizehaber.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
