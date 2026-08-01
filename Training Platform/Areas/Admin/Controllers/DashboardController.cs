using Microsoft.AspNetCore.Mvc;

namespace Training_Platform.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
