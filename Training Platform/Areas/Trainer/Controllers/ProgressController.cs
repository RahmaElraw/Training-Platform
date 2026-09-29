using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Training_Platform.Areas.Trainer.Controllers
{
    [Area(SD.Trainer_Area)]
    public class ProgressController : Controller
    {
        private readonly IProgressRepository _progressRepository;
        private readonly IRepository<Course> _courseRepository;

        public ProgressController(
            IProgressRepository progressRepository,
            IRepository<Course> courseRepository)
        {
            _progressRepository = progressRepository;
            _courseRepository = courseRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? courseId,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            // Get trainer courses for filter
            var courses = await _courseRepository.GetAsync(
                c => c.TrainerId == trainerId,
                tracked: false,
                cancellationToken: cancellationToken);

            courses = courses
                .OrderBy(c => c.Title)
                .ToList();

            ViewBag.Courses = courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Title,
                Selected = courseId.HasValue &&
                           c.Id == courseId.Value
            }).ToList();

            // Get progress
            var progress =
                await _progressRepository.GetTrainerProgressAsync(
                    trainerId,
                    courseId,
                    cancellationToken);

            return View(progress);
        }

        private int GetCurrentTrainerId()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            return int.TryParse(
                userId,
                out int trainerId)
                ? trainerId
                : 0;
        }
    }
}