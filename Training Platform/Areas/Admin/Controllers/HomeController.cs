using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Training_Platform.Models;
using Training_Platform.Repositories.IRepositories;
using Training_Platform.ViewModels;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    public class HomeController : Controller
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            IRepository<Category> categoryRepository,
            IRepository<Course> courseRepository,
            UserManager<ApplicationUser> userManager)
        {
            _categoryRepository = categoryRepository;
            _courseRepository = courseRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var trainees = await _userManager.GetUsersInRoleAsync("Trainee");
            var trainers = await _userManager.GetUsersInRoleAsync("Trainer");

            var categories = await _categoryRepository.GetAsync(
                tracked: false);

            var courses = await _courseRepository.GetAsync(
                tracked: false);

            var model = new AdminDashboardVM
            {
                TraineesCount = trainees.Count,
                TrainersCount = trainers.Count,
                CategoriesCount = categories.Count(),
                CoursesCount = courses.Count()
            };

            return View(model);
        }
    }
}