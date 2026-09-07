using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Training_Platform.ViewModels.Trainee;

namespace Training_Platform.Areas.Trainee.Controllers
{
    [Area(SD.Trainee_Area)]
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Enrollment> _enrollmentRepository;
        private readonly IRepository<UserProgress> _progressRepository;
        private readonly IRepository<Review> _reviewRepository;
        private readonly IRepository<Category> _categoryRepository;

        public CoursesController(
            IRepository<Course> courseRepository,
            IRepository<Enrollment> enrollmentRepository,
            IRepository<UserProgress> progressRepository,
            IRepository<Review> reviewRepository,
            IRepository<Category> categoryRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _progressRepository = progressRepository;
            _reviewRepository = reviewRepository;
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
    string? query = null,
    int? categoryId = null,
    string? level = null,
    int page = 1,
    CancellationToken cancellationToken = default)
        {
            query = query?.Trim();
            int pageSize = 4; // Updated page size to 4

            var allCourses = await _courseRepository.GetAsync(
                c =>
                    c.IsPublished &&
                    (
                        string.IsNullOrWhiteSpace(query) ||
                        c.Title.Contains(query) ||
                        c.Description.Contains(query)
                    ) &&
                    (!categoryId.HasValue || c.CategoryId == categoryId.Value) &&
                    (string.IsNullOrWhiteSpace(level) || c.Level.ToString() == level),
                includes:
                [
                    c => c.Category,
            c => c.Lessons,
            c => c.Reviews
                ],
                tracked: false,
                cancellationToken: cancellationToken
            );

            var categories = await _categoryRepository.GetAsync(
                tracked: false,
                cancellationToken: cancellationToken
            );

            var totalItems = allCourses.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginatedCourses = allCourses
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Query = query;
            ViewBag.CategoryId = categoryId;
            ViewBag.Level = level;
            ViewBag.Categories = categories.OrderBy(c => c.Name).ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(paginatedCourses);
        }


        [HttpGet]
        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            // Get Course + Category + Trainer + Lessons
            var course = await _courseRepository.GetOneAsync(
                c =>
                    c.Id == id &&
                    c.IsPublished,
                includes:
                [
                    c => c.Category,
            c => c.Trainer,
            c => c.Lessons,
            c => c.Reviews
                ],
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (course is null)
                return NotFound();

            // Check Enrollment
            var enrollment = await _enrollmentRepository.GetOneAsync(
                e =>
                    e.UserId == userId &&
                    e.CourseId == id,
                tracked: false,
                cancellationToken: cancellationToken
            );

            var completedLessons = 0;
            var lessons = course.Lessons.OrderBy(l => l.OrderNumber).ToList();
            var totalLessons = lessons.Count;

            if (enrollment is not null && totalLessons > 0)
            {
                var lessonIds = lessons.Select(l => l.Id).ToList();

                // Query progress using lesson IDs instead of p.Lesson.CourseId
                var progress = await _progressRepository.GetAsync(
                    p =>
                        p.UserId == userId &&
                        p.IsCompleted &&
                        lessonIds.Contains(p.LessonId),
                    tracked: false,
                    cancellationToken: cancellationToken
                );

                completedLessons = progress.Select(p => p.LessonId).Distinct().Count();
            }

            // Determine actual completion status
            bool isCourseCompleted = totalLessons > 0 && completedLessons == totalLessons;

            // Fix/Sync stale enrollment record in DB if out of sync
            if (enrollment is not null && enrollment.IsCompleted != isCourseCompleted)
            {
                var trackedEnrollment = await _enrollmentRepository.GetOneAsync(
                    e => e.Id == enrollment.Id,
                    tracked: true,
                    cancellationToken: cancellationToken);

                if (trackedEnrollment is not null)
                {
                    trackedEnrollment.IsCompleted = isCourseCompleted;
                    await _enrollmentRepository.CommitAsync(cancellationToken);
                }
            }

            var userReview = course.Reviews.FirstOrDefault(r => r.UserId == userId);
            double avgRating = course.Reviews.Any() ? course.Reviews.Average(r => r.Rating) : 0;

            var vm = new CourseDetailsVM
            {
                Course = course,
                IsEnrolled = enrollment is not null,
                IsCompleted = isCourseCompleted, // Uses dynamically verified status
                CompletedLessons = completedLessons,
                TotalLessons = totalLessons,
                AverageRating = Math.Round(avgRating, 1),
                TotalReviews = course.Reviews.Count,
                UserRating = userReview?.Rating ?? 0,
                UserComment = userReview?.Comment
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(
            int courseId,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();


            var course = await _courseRepository.GetOneAsync(
                c =>
                    c.Id == courseId &&
                    c.IsPublished,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (course is null)
                return NotFound();


            var existingEnrollment =
                await _enrollmentRepository.GetOneAsync(
                    e =>
                        e.UserId == userId &&
                        e.CourseId == courseId,
                    tracked: false,
                    cancellationToken: cancellationToken
                );


            if (existingEnrollment is not null)
            {
                TempData["Error"] =
                    "You are already enrolled in this course.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = courseId });
            }


            // Create Enrollment
            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrollmentDate = DateTime.UtcNow,
                IsCompleted = false
            };


            await _enrollmentRepository.AddAsync(
                enrollment,
                cancellationToken);


            await _enrollmentRepository.CommitAsync(
                cancellationToken);


            TempData["Success"] =
                "You have enrolled successfully.";


            return RedirectToAction(
                nameof(Details),
                new { id = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(
        int courseId,
        int rating,
        string? comment,
        CancellationToken cancellationToken = default)
        {
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Rating must be between 1 and 5 stars.";
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            var userId = GetCurrentUserId();

            var enrollment = await _enrollmentRepository.GetOneAsync(
                e => e.UserId == userId && e.CourseId == courseId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (enrollment is null)
            {
                TempData["Error"] = "Only enrolled trainees can leave a review.";
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            var existingReview = await _reviewRepository.GetOneAsync(
                r => r.UserId == userId && r.CourseId == courseId,
                tracked: true,
                cancellationToken: cancellationToken
            );

            if (existingReview is not null)
            {
                existingReview.Rating = rating;
                existingReview.Comment = comment;
                existingReview.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                var newReview = new Review
                {
                    CourseId = courseId,
                    UserId = userId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                };
                await _reviewRepository.AddAsync(newReview, cancellationToken);
            }

            await _reviewRepository.CommitAsync(cancellationToken);

            TempData["Success"] = "Your review has been saved.";
            return RedirectToAction(nameof(Details), new { id = courseId });
        }


        // Helper
        private int GetCurrentUserId()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException();

            if (!int.TryParse(userId, out var id))
                throw new UnauthorizedAccessException();

            return int.Parse(userId);
        }
    }
}
