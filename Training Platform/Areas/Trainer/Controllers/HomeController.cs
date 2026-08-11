using Microsoft.AspNetCore.Mvc;

namespace Training_Platform.Areas.Trainer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
