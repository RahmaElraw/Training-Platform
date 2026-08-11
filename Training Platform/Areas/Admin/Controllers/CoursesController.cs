using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    public class CoursesController : Controller
    {
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ApplicationUser> _userRepository;

        private const int PageSize = 6;

        public CoursesController(
            IRepository<Course> courseRepository,
            IRepository<Category> categoryRepository,
            IRepository<ApplicationUser> userRepository)
        {
            _courseRepository = courseRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
        }
        public async Task<IActionResult> Index(
            string? query,
            int page = 1)
        {
            var courses = await _courseRepository.GetAsync(
                includes:
                [
                    c => c.Category,
                    c => c.Trainer
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

            courses = courses.OrderBy(c => c.Title);

            int totalCount = courses.Count();

            var model = new CourseWithRelatedVM
            {
                Courses = courses
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize),

                CurrentPage = page,

                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)PageSize),

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
            if (!ModelState.IsValid)
            {
                await LoadCourseData(model);
                return View(model);
            }
            var category = await _categoryRepository.GetOneAsync(
                c => c.Id == model.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Selected category does not exist.");

                await LoadCourseData(model);
                return View(model);
            }
            var trainer = await _userRepository.GetOneAsync(
                u => u.Id == model.TrainerId);

            if (trainer == null)
            {
                ModelState.AddModelError(
                    nameof(model.TrainerId),
                    "Selected trainer does not exist.");

                await LoadCourseData(model);
                return View(model);
            }
            var exists = await _courseRepository.GetOneAsync(
                c => c.Title.ToLower().Trim()
                     == model.Title.ToLower().Trim());

            if (exists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "Course title already exists.");

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

                TrainerId = model.TrainerId
            };

            await _courseRepository.AddAsync(course);

            if (await _courseRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Course created successfully.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] =
                "Something went wrong.";

            await LoadCourseData(model);

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseRepository.GetOneAsync(
                c => c.Id == id);

            if (course == null)
                return NotFound();

            var model = new CourseVM
            {
                Id = course.Id,

                Title = course.Title,

                Description = course.Description,

                DurationInHours = course.DurationInHours,

                Thumbnail = course.Thumbnail,

                Level = course.Level,

                IsPublished = course.IsPublished,

                CategoryId = course.CategoryId,

                TrainerId = course.TrainerId
            };

            await LoadCourseData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCourseData(model);
                return View(model);
            }

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == model.Id);

            if (course == null)
                return NotFound();
            var category = await _categoryRepository.GetOneAsync(
                c => c.Id == model.CategoryId);

            if (category == null)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Selected category does not exist.");

                await LoadCourseData(model);
                return View(model);
            }
            var trainer = await _userRepository.GetOneAsync(
                u => u.Id == model.TrainerId);

            if (trainer == null)
            {
                ModelState.AddModelError(
                    nameof(model.TrainerId),
                    "Selected trainer does not exist.");

                await LoadCourseData(model);
                return View(model);
            }
            var exists = await _courseRepository.GetOneAsync(
                c => c.Title.ToLower().Trim()
                     == model.Title.ToLower().Trim()
                     && c.Id != model.Id);

            if (exists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "Course title already exists.");

                await LoadCourseData(model);
                return View(model);
            }

            course.Title = model.Title.Trim();

            course.Description = model.Description.Trim();

            course.DurationInHours =
                model.DurationInHours;

            course.Thumbnail =
                model.Thumbnail?.Trim();

            course.Level = model.Level;

            course.IsPublished =
                model.IsPublished;

            course.CategoryId =
                model.CategoryId;

            course.TrainerId =
                model.TrainerId;

            _courseRepository.Update(course);

            if (await _courseRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Course updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] =
                "Something went wrong.";

            await LoadCourseData(model);

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> RelatedCourses(int id)
        {
            var course = await _courseRepository.GetOneAsync(
                c => c.Id == id,
                tracked: false);

            if (course == null)
                return NotFound();
            var relatedCourses = await _courseRepository.GetAsync(
                c => c.CategoryId == course.CategoryId
                     && c.Id != course.Id
                     && c.IsPublished,
                includes:
                [
                    c => c.Category,
                    c => c.Trainer
                ],
                tracked: false);

            relatedCourses = relatedCourses
                .OrderBy(c => c.Title);

            return View(relatedCourses);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetOneAsync(
                c => c.Id == id,
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

            if (course.Enrollments.Count > 0)
            {
                TempData["Error"] =
                    "Cannot delete course because it has enrollments.";

                return RedirectToAction(nameof(Index));
            }

            if (course.Lessons.Count > 0)
            {
                TempData["Error"] =
                    "Cannot delete course because it has lessons.";

                return RedirectToAction(nameof(Index));
            }

            if (course.Quizzes.Count > 0)
            {
                TempData["Error"] =
                    "Cannot delete course because it has quizzes.";

                return RedirectToAction(nameof(Index));
            }

            if (course.Certificates.Count > 0)
            {
                TempData["Error"] =
                    "Cannot delete course because it has certificates.";

                return RedirectToAction(nameof(Index));
            }

            if (course.Reviews.Count > 0)
            {
                TempData["Error"] =
                    "Cannot delete course because it has reviews.";

                return RedirectToAction(nameof(Index));
            }

            _courseRepository.Delete(course);

            if (await _courseRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Course deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Something went wrong.";
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseRepository.GetOneAsync(
                c => c.Id == id,
                includes:
                [
                    c => c.Category,
                    c => c.Trainer,
                    c => c.Lessons,
                    c => c.Quizzes,
                    c => c.Enrollments,
                    c => c.Reviews,
                    c => c.Certificates
                ]);

            if (course == null)
                return NotFound();

            return View(course);
        }

        private async Task LoadCourseData(
            CourseVM? model = null)
        {
            var categories =
                await _categoryRepository.GetAsync(
                    tracked: false);

            var trainers =
                await _userRepository.GetAsync(
                    tracked: false);

            ViewBag.Categories =
                new SelectList(
                    categories,
                    "Id",
                    "Name",
                    model?.CategoryId);

            ViewBag.Trainers =
                new SelectList(
                    trainers,
                    "Id",
                    "UserName",
                    model?.TrainerId);
        }
    }
}
