using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Training_Platform.ViewModels.Trainee;

namespace Training_Platform.Areas.Trainee.Controllers
{
    [Area(SD.Trainee_Area)]
    [Authorize]
    public class EnrollmentsController : Controller
    {
        private readonly IRepository<Enrollment> _enrollmentRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Review> _reviewRepository;

        public EnrollmentsController(
            IRepository<Enrollment> enrollmentRepository,
            IRepository<Course> courseRepository,
            IRepository<Review> reviewRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _reviewRepository = reviewRepository;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!);
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var enrollments = await _enrollmentRepository.GetAsync(
                e => e.UserId == userId,
                includes: [e => e.Course, e => e.Course.Lessons],
                tracked: false,
                cancellationToken: cancellationToken
            );

            var reviews = await _reviewRepository.GetAsync(
                r => r.UserId == userId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            var reviewMap = reviews.ToDictionary(r => r.CourseId, r => r.Id);

            var vmList = enrollments.Select(e => new EnrollmentItemVM
            {
                EnrollmentId = e.Id,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                CourseThumbnail = e.Course.Thumbnail,
                Level = e.Course.Level.ToString(),
                EnrollmentDate = e.EnrollmentDate,
                IsCompleted = e.IsCompleted,
                TotalLessons = e.Course.Lessons?.Count ?? 0,
                HasReviewed = reviewMap.ContainsKey(e.CourseId),
                ReviewId = reviewMap.TryGetValue(e.CourseId, out var reviewId) ? reviewId : null
            })
            .OrderByDescending(e => e.EnrollmentDate)
            .ToList();

            return View(vmList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == courseId && c.IsPublished,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (course is null)
                return NotFound();

            var existingEnrollment = await _enrollmentRepository.GetOneAsync(
                e => e.UserId == userId && e.CourseId == courseId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (existingEnrollment is not null)
            {
                TempData["Info"] = "You are already enrolled in this course.";
                return RedirectToAction(nameof(Index));
            }

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrollmentDate = DateTime.UtcNow,
                IsCompleted = false
            };

            await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
            await _enrollmentRepository.CommitAsync(cancellationToken);

            TempData["Success"] = "Enrolled successfully! Enjoy learning.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(int id, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var enrollment = await _enrollmentRepository.GetOneAsync(
                e => e.Id == id && e.UserId == userId,
                tracked: true,
                cancellationToken: cancellationToken
            );

            if (enrollment is not null)
            {
                _enrollmentRepository.Delete(enrollment);
                await _enrollmentRepository.CommitAsync(cancellationToken);
                TempData["Success"] = "You have unenrolled from the course.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}