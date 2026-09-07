using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Training_Platform.ViewModels.Trainee;

namespace Training_Platform.Areas.Trainee.Controllers
{
    [Area(SD.Trainee_Area)]
    [Authorize] 
    public class ReviewsController : Controller
    {
        private readonly IRepository<Review> _reviewRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Enrollment> _enrollmentRepository;

        public ReviewsController(
            IRepository<Review> reviewRepository,
            IRepository<Course> courseRepository,
            IRepository<Enrollment> enrollmentRepository)
        {
            _reviewRepository = reviewRepository;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var reviews = await _reviewRepository.GetAsync(
                r => r.UserId == userId,
                includes: [r => r.Course],
                tracked: false,
                cancellationToken: cancellationToken
            );

            var result = reviews.OrderByDescending(r => r.CreatedAt).ToList();

            return View(result);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<IActionResult> Create(
            int courseId,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == courseId && c.IsPublished,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (course is null)
                return NotFound();

            var enrollment = await _enrollmentRepository.GetOneAsync(
                e => e.UserId == userId && e.CourseId == courseId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (enrollment is null)
            {
                TempData["Error"] = "You must be enrolled in this course to leave a review.";
                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            var existingReview = await _reviewRepository.GetOneAsync(
                r => r.UserId == userId && r.CourseId == courseId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (existingReview is not null)
            {
                TempData["Info"] = "You have already reviewed this course.";
                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            var vm = new ReviewCreateVM
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                Rating = 5 // Default 
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ReviewCreateVM vm,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var userId = GetCurrentUserId();

            var enrollment = await _enrollmentRepository.GetOneAsync(
                e => e.UserId == userId && e.CourseId == vm.CourseId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (enrollment is null)
            {
                return Forbid();
            }

            var review = new Review
            {
                CourseId = vm.CourseId,
                UserId = userId,
                Rating = vm.Rating,
                Comment = vm.Comment,
                CreatedAt = DateTime.UtcNow 
            };

            await _reviewRepository.AddAsync(review, cancellationToken);
            await _reviewRepository.CommitAsync(cancellationToken);

            TempData["Success"] = "Thank you! Your review has been submitted.";
            return RedirectToAction("Details", "Courses", new { id = vm.CourseId });
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var review = await _reviewRepository.GetOneAsync(
                r => r.Id == id && r.UserId == userId,
                includes: [r => r.Course],
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (review is null)
                return NotFound();

            var vm = new ReviewEditVM
            {
                Id = review.Id,
                CourseTitle = review.Course.Title,
                Rating = review.Rating,
                Comment = review.Comment
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReviewEditVM vm, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var userId = GetCurrentUserId();

            // Fetch the entity with tracking enabled so EF Core knows we are updating it
            var review = await _reviewRepository.GetOneAsync(
                r => r.Id == vm.Id && r.UserId == userId,
                tracked: true,
                cancellationToken: cancellationToken
            );

            if (review is null)
                return NotFound();

            review.Rating = vm.Rating;
            review.Comment = vm.Comment;

            _reviewRepository.Update(review);
            await _reviewRepository.CommitAsync(cancellationToken);

            TempData["Success"] = "Your review has been updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var review = await _reviewRepository.GetOneAsync(
                r => r.Id == id && r.UserId == userId,
                tracked: true,
                cancellationToken: cancellationToken
            );

            if (review is not null)
            {
                _reviewRepository.Delete(review);
                await _reviewRepository.CommitAsync(cancellationToken);
                TempData["Success"] = "Your review was successfully deleted.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}