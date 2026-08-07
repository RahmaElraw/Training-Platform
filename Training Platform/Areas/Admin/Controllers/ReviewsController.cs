using Microsoft.AspNetCore.Mvc;
using Training_Platform.Models;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    public class ReviewsController : Controller
    {
        private readonly IRepository<Review> _reviewRepository;

        public ReviewsController(IRepository<Review> reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            string? query = null,
            CancellationToken cancellationToken = default)
        {
            var reviews = await _reviewRepository.GetAsync(
                includes:
                [
                    r => r.Course,
                    r => r.User
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim().ToLower();

                reviews = reviews.Where(r =>
                    r.Course.Title.ToLower().Contains(query) ||
                    r.User.UserName!.ToLower().Contains(query) ||
                    (r.Comment != null && r.Comment.ToLower().Contains(query)));
            }

            const int pageSize = 6;

            int totalPages = (int)Math.Ceiling(reviews.Count() / (double)pageSize);

            reviews = reviews
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return View(new ReviewWithRelatedVM
            {
                Reviews = reviews,
                CurrentPage = page,
                TotalPages = totalPages,
                Query = query
            });
        }

        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken = default)
        {
            var review = await _reviewRepository.GetOneAsync(
                r => r.Id == id,
                includes:
                [
                    r => r.Course,
            r => r.User
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            if (review == null)
                return NotFound();

            var model = new ReviewVM
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                CourseId = review.CourseId,
                UserId = review.UserId.ToString(), //UserId = review.UserId; until it inherits from IdentityUser
                CourseTitle = review.Course.Title,
                UserName = review.User.UserName!
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken = default)
        {
            var review = await _reviewRepository.GetOneAsync(
                r => r.Id == id,
                cancellationToken: cancellationToken);

            if (review == null)
                return NotFound();

            _reviewRepository.Delete(review);

            if (await _reviewRepository.CommitAsync(cancellationToken) > 0)
            {
                TempData["Success"] = "Review deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Something went wrong.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}