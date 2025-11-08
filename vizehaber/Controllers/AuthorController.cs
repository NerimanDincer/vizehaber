using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace vizehaber.Controllers
{
    [Authorize]
    public class AuthorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
