using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Training_Platform.ViewModels.Trainer;

namespace Training_Platform.Areas.Trainer.Controllers
{
    [Area(SD.Trainer_Area)]
    public class CoursesController : Controller
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Category> _categoryRepository;

        private const int PageSize = 6;

        public CoursesController(
            IRepository<Course> courseRepository,
            IRepository<Category> categoryRepository)
        {
            _courseRepository = courseRepository;
            _categoryRepository = categoryRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Index(
            string? query,
            int page = 1)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            if (page < 1)
                page = 1;

            var courses = await _courseRepository.GetAsync(
                c => c.TrainerId == trainerId,
                includes:
                [
                    c => c.Category
                ],
                tracked: false);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();

                courses = courses.Where(c =>
                    c.Title.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase));
            }

            courses = courses
                .OrderByDescending(c => c.CreatedAt);

            int totalCount = courses.Count();

            int totalPages =
                (int)Math.Ceiling(
                    totalCount / (double)PageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var model = new CourseIndexVM
            {
                Courses = courses
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize),

                CurrentPage = page,

                TotalPages = totalPages,

                Query = query
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCourseData();

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseVM model)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            model.TrainerId = trainerId;

            if (!ModelState.IsValid)
            {
                await LoadCourseData(model);
                return View(model);
            }

            var category =
                await _categoryRepository.GetOneAsync(
                    c => c.Id == model.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Selected category does not exist.");

                await LoadCourseData(model);
                return View(model);
            }

            var exists =
                await _courseRepository.GetOneAsync(
                    c =>
                        c.TrainerId == trainerId &&
                        c.Title.ToLower().Trim()
                        ==
                        model.Title.ToLower().Trim());

            if (exists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "You already have a course with this title.");

                await LoadCourseData(model);
                return View(model);
            }

            var course = new Course
            {
                Title = model.Title.Trim(),

                Description = model.Description.Trim(),

                DurationInHours = model.DurationInHours,

                Thumbnail = model.Thumbnail?.Trim(),

                Level = model.Level,

                IsPublished = model.IsPublished,

                CreatedAt = DateTime.UtcNow,

                CategoryId = model.CategoryId,

                TrainerId = trainerId
            };

            await _courseRepository.AddAsync(course);

            if (await _courseRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Course created successfully.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] =
                "Something went wrong while creating the course.";

            await LoadCourseData(model);

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course =
                await _courseRepository.GetOneAsync(
                    c =>
                        c.Id == id &&
                        c.TrainerId == trainerId);

            if (course == null)
                return NotFound();

            var model = new CourseVM
            {
                Id = course.Id,

                Title = course.Title,

                Description = course.Description,

                DurationInHours =
                    course.DurationInHours,

                Thumbnail =
                    course.Thumbnail,

                Level =
                    course.Level,

                IsPublished =
                    course.IsPublished,

                CategoryId =
                    course.CategoryId,

                TrainerId =
                    trainerId
            };

            await LoadCourseData(model);

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            CourseVM model)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            model.TrainerId = trainerId;

            if (!ModelState.IsValid)
            {
                await LoadCourseData(model);
                return View(model);
            }

            var course =
                await _courseRepository.GetOneAsync(
                    c =>
                        c.Id == model.Id &&
                        c.TrainerId == trainerId);

            if (course == null)
                return NotFound();

            var category =
                await _categoryRepository.GetOneAsync(
                    c => c.Id == model.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Selected category does not exist.");

                await LoadCourseData(model);
                return View(model);
            }

            var exists =
                await _courseRepository.GetOneAsync(
                    c =>
                        c.TrainerId == trainerId &&
                        c.Title.ToLower().Trim()
                        ==
                        model.Title.ToLower().Trim()
                        &&
                        c.Id != model.Id);

            if (exists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "You already have a course with this title.");

                await LoadCourseData(model);
                return View(model);
            }

            course.Title =
                model.Title.Trim();

            course.Description =
                model.Description.Trim();

            course.DurationInHours =
                model.DurationInHours;

            course.Thumbnail =
                model.Thumbnail?.Trim();

            course.Level =
                model.Level;

            course.IsPublished =
                model.IsPublished;

            course.CategoryId =
                model.CategoryId;

            course.TrainerId = trainerId;

            _courseRepository.Update(course);

            if (await _courseRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Course updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] =
                "Something went wrong while updating the course.";

            await LoadCourseData(model);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course =
                await _courseRepository.GetOneAsync(
                    c =>
                        c.Id == id &&
                        c.TrainerId == trainerId,
                    includes:
                    [
                        c => c.Category,
                        c => c.Lessons,
                        c => c.Quizzes,
                        c => c.Enrollments,
                        c => c.Reviews,
                        c => c.Certificates
                    ],
                    tracked: false);

            if (course == null)
                return NotFound();

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == id &&
                     c.TrainerId == trainerId,
                includes:
                [
                    c => c.Lessons,
            c => c.Quizzes,
            c => c.Enrollments,
            c => c.Certificates,
            c => c.Reviews
                ]);

            if (course == null)
                return NotFound();

            int lessonsCount = course.Lessons?.Count ?? 0;
            int quizzesCount = course.Quizzes?.Count ?? 0;
            int enrollmentsCount = course.Enrollments?.Count ?? 0;
            int certificatesCount = course.Certificates?.Count ?? 0;
            int reviewsCount = course.Reviews?.Count ?? 0;

            if (certificatesCount > 0 || reviewsCount > 0)
            {
                TempData["Error"] =
                    "This course cannot be deleted because it has " +
                    $"{certificatesCount} certificate(s) and " +
                    $"{reviewsCount} review(s).";

                return RedirectToAction(nameof(Index));
            }

            _courseRepository.Delete(course);

            if (await _courseRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    $"Course deleted successfully. " +
                    $"{lessonsCount} lesson(s), " +
                    $"{quizzesCount} quiz(zes), and " +
                    $"{enrollmentsCount} enrollment(s) were also deleted.";
            }
            else
            {
                TempData["Error"] =
                    "Something went wrong while deleting the course.";
            }

            return RedirectToAction(nameof(Index));
        }

        private int GetCurrentTrainerId()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            return int.TryParse(
                userId,
                out int trainerId)
                ? trainerId
                : 0;
        }

        private async Task LoadCourseData(
            CourseVM? model = null)
        {
            var categories =
                await _categoryRepository.GetAsync(
                    tracked: false);

            ViewBag.Categories =
                new SelectList(
                    categories.OrderBy(c => c.Name),
                    "Id",
                    "Name",
                    model?.CategoryId);
        }
    }
}