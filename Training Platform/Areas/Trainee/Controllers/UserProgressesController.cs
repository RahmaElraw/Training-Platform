using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Training_Platform.Areas.Trainee.Controllers
{
    [Area(SD.Trainee_Area)]
    [Authorize]
    public class UserProgressesController : Controller
    {
        private readonly IRepository<UserProgress> _progressRepository;
        private readonly IRepository<Enrollment> _enrollmentRepository;
        private readonly IRepository<Lesson> _lessonRepository;

        public UserProgressesController(
            IRepository<UserProgress> progressRepository,
            IRepository<Enrollment> enrollmentRepository,
            IRepository<Lesson> lessonRepository)
        {
            _progressRepository = progressRepository;
            _enrollmentRepository = enrollmentRepository;
            _lessonRepository = lessonRepository;
        }

        public async Task<IActionResult> Index(
            int courseId,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var enrollment = await _enrollmentRepository.GetOneAsync(
                e => e.UserId == userId &&
                     e.CourseId == courseId,
                tracked: false,
                cancellationToken: cancellationToken);

            if (enrollment is null)
                return Unauthorized();

            var lessons = await _lessonRepository.GetAsync(
                l => l.CourseId == courseId,
                tracked: false,
                cancellationToken: cancellationToken);

            var lessonIds = lessons
                .Select(l => l.Id)
                .ToList();

            var progress = await _progressRepository.GetAsync(
                p => p.UserId == userId &&
                     lessonIds.Contains(p.LessonId),
                tracked: false,
                cancellationToken: cancellationToken);

            return View(progress);
        }

        private int GetCurrentUserId()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var id))
                throw new UnauthorizedAccessException();

            return id;
        }
    }
}