using Microsoft.AspNetCore.Mvc;

namespace Training_Platform.Areas.Trainer.Controllers
{
    [Area(SD.Trainer_Area)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
