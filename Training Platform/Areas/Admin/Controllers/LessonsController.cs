using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    [Authorize(Roles = $"{RoleNames.SUPER_ADMIN}")]

    public class LessonsController : Controller
    {
        private readonly IRepository<Lesson> _lessonRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<CourseMaterial> _courseMaterialRepository;
        private const int PageSize = 6;

        public LessonsController(
            IRepository<Lesson> lessonRepository,
            IRepository<Course> courseRepository,
            IRepository<CourseMaterial> courseMaterialRepository)
        {
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _courseMaterialRepository = courseMaterialRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(
            string? query,
            int page = 1)
        {
            var lessons = await _lessonRepository.GetAsync(
                includes:
                [
                    l => l.Course,
                    l => l.CourseMaterials,
                    l => l.UserProgresses
                ],
                tracked: false);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();

                lessons = lessons.Where(l =>
                    l.Title.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase));
            }

            lessons = lessons
                .OrderBy(l => l.Course.Title)
                .ThenBy(l => l.OrderNumber);

            int totalCount = lessons.Count();

            var model = new LessonWithRelatedVM
            {
                Lessons = lessons
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
            await LoadLessonData();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LessonVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadLessonData(model);
                return View(model);
            }


            // Check Course
            var course = await _courseRepository.GetOneAsync(
                c => c.Id == model.CourseId);

            if (course == null)
            {
                ModelState.AddModelError(
                    nameof(model.CourseId),
                    "Selected course does not exist.");

                await LoadLessonData(model);
                return View(model);
            }
            var orderExists = await _lessonRepository.GetOneAsync(
                l => l.CourseId == model.CourseId
                     && l.OrderNumber == model.OrderNumber);

            if (orderExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.OrderNumber),
                    "This order number already exists in this course.");

                await LoadLessonData(model);
                return View(model);
            }


            var lesson = new Lesson
            {
                Title = model.Title.Trim(),

                Description = model.Description?.Trim(),

                VideoUrl = model.VideoUrl.Trim(),

                OrderNumber = model.OrderNumber,

                CourseId = model.CourseId
            };


            await _lessonRepository.AddAsync(lesson);


            if (await _lessonRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Lesson created successfully.";

                return RedirectToAction(nameof(Index));
            }


            TempData["Error"] =
                "Something went wrong.";

            await LoadLessonData(model);

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == id,
                includes:
                [
                    l => l.CourseMaterials
                ]);

            if (lesson == null)
                return NotFound();

            var model = new LessonVM
            {
                Id = lesson.Id,

                Title = lesson.Title,

                Description = lesson.Description,

                VideoUrl = lesson.VideoUrl,

                OrderNumber = lesson.OrderNumber,

                CourseId = lesson.CourseId,

                CourseMaterials = lesson.CourseMaterials
            };

            await LoadLessonData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LessonVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadLessonData(model);
                return View(model);
            }


            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == model.Id);

            if (lesson == null)
                return NotFound();
            var course = await _courseRepository.GetOneAsync(
                c => c.Id == model.CourseId);

            if (course == null)
            {
                ModelState.AddModelError(
                    nameof(model.CourseId),
                    "Selected course does not exist.");

                await LoadLessonData(model);
                return View(model);
            }
            var orderExists = await _lessonRepository.GetOneAsync(
                l => l.CourseId == model.CourseId
                     && l.OrderNumber == model.OrderNumber
                     && l.Id != model.Id);

            if (orderExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.OrderNumber),
                    "This order number already exists in this course.");

                await LoadLessonData(model);
                return View(model);
            }


            lesson.Title = model.Title.Trim();

            lesson.Description =
                model.Description?.Trim();

            lesson.VideoUrl =
                model.VideoUrl.Trim();

            lesson.OrderNumber =
                model.OrderNumber;

            lesson.CourseId =
                model.CourseId;


            _lessonRepository.Update(lesson);


            if (await _lessonRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Lesson updated successfully.";

                return RedirectToAction(nameof(Index));
            }


            TempData["Error"] =
                "Something went wrong.";

            await LoadLessonData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == id,
                includes:
                [
                    l => l.CourseMaterials,
                    l => l.UserProgresses
                ]);


            if (lesson == null)
                return NotFound();


            if (lesson.CourseMaterials.Count > 0)
            {
                TempData["Error"] =
                    "Cannot delete lesson because it has course materials.";

                return RedirectToAction(nameof(Index));
            }


            if (lesson.UserProgresses.Count > 0)
            {
                TempData["Error"] =
                    "Cannot delete lesson because it has user progress records.";

                return RedirectToAction(nameof(Index));
            }


            _lessonRepository.Delete(lesson);


            if (await _lessonRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Lesson deleted successfully.";
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
            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == id,
                includes:
                [
                    l => l.Course,
                    l => l.CourseMaterials,
                    l => l.UserProgresses
                ]);


            if (lesson == null)
                return NotFound();


            return View(lesson);
        }
        private async Task LoadLessonData(
            LessonVM? model = null)
        {
            var courses =
                await _courseRepository.GetAsync(
                    tracked: false);


            ViewBag.Courses =
                new SelectList(
                    courses.OrderBy(c => c.Title),
                    "Id",
                    "Title",
                    model?.CourseId);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(
            CourseMaterialVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Please enter valid material data.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.LessonId });
            }

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == model.LessonId);

            if (lesson == null)
                return NotFound();

            var material = new CourseMaterial
            {
                Title = model.Title.Trim(),
                Url = model.Url.Trim(),
                LessonId = model.LessonId
            };

            await _courseMaterialRepository.AddAsync(material);

            if (await _courseMaterialRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Course material added successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Something went wrong.";
            }

            return RedirectToAction(
                nameof(Details),
                new { id = model.LessonId });
        }
        [HttpGet]
        public async Task<IActionResult> EditMaterial(int id)
        {
            var material =
                await _courseMaterialRepository.GetOneAsync(
                    m => m.Id == id);

            if (material == null)
                return NotFound();

            var model = new CourseMaterialVM
            {
                Id = material.Id,
                Title = material.Title ?? string.Empty,
                Url = material.Url,
                LessonId = material.LessonId
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaterial(
            CourseMaterialVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var material =
                await _courseMaterialRepository.GetOneAsync(
                    m => m.Id == model.Id);

            if (material == null)
                return NotFound();

            var lesson =
                await _lessonRepository.GetOneAsync(
                    l => l.Id == model.LessonId);

            if (lesson == null)
                return NotFound();

            material.Title = model.Title.Trim();

            material.Url = model.Url.Trim();

            material.LessonId = model.LessonId;

            _courseMaterialRepository.Update(material);

            if (await _courseMaterialRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Course material updated successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.LessonId });
            }

            TempData["Error"] =
                "Something went wrong.";

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var material =
                await _courseMaterialRepository.GetOneAsync(
                    m => m.Id == id);

            if (material == null)
                return NotFound();

            int lessonId = material.LessonId;

            _courseMaterialRepository.Delete(material);

            if (await _courseMaterialRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Course material deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Something went wrong.";
            }

            return RedirectToAction(
                nameof(Details),
                new { id = lessonId });
        }
    }
}
