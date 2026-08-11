using Microsoft.AspNetCore.Mvc;

namespace Training_Platform.Areas.Trainee.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
