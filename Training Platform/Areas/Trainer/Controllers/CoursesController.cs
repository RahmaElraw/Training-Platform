using System.Security.Claims;

namespace Training_Platform.Areas.Trainer.Controllers
{
    [Area(SD.Trainer_Area)]
    public class CoursesController : Controller
    {
        private readonly IRepository<Course> _courseRepository;

        public CoursesController(IRepository<Course> courseRepository)
        {
            _courseRepository = courseRepository;
        }

        // GET: Trainer/Courses
        [HttpGet]
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var courses = await _courseRepository.GetAsync(
                c => c.TrainerId == trainerId,
                includes:
                [
                    c => c.Category
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            var orderedCourses = courses
                .OrderByDescending(c => c.CreatedAt);

            return View(orderedCourses);
        }

        // GET: Trainer/Courses/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course = await _courseRepository.GetOneAsync(
    c => c.Id == id &&
         c.TrainerId == trainerId,
    includes:
    [
        c => c.Category,
        c => c.Lessons,
        c => c.Quizzes
    ],
    tracked: false,
    cancellationToken: cancellationToken);

            if (course == null)
                return NotFound();

            return View(course);
        }

        private int GetCurrentTrainerId()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            return int.TryParse(userId, out int trainerId)
                ? trainerId
                : 0;
        }
    }
}