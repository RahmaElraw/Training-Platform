using System.Security.Claims;
using Microsoft.Extensions.Localization;

namespace Training_Platform.Areas.Trainer.Controllers
{
    [Area(SD.Trainer_Area)]
    [Authorize(Roles = RoleNames.TRAINER)]
    public class LessonsController : Controller
    {
        private readonly IRepository<Lesson> _lessonRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public LessonsController(
            IRepository<Lesson> lessonRepository,
            IRepository<Course> courseRepository,
            IStringLocalizer<SharedResource> localizer)
        {
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int courseId,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == courseId &&
                     c.TrainerId == trainerId,
                tracked: false,
                cancellationToken: cancellationToken);

            if (course == null)
                return NotFound();

            var lessons = await _lessonRepository.GetAsync(
                l => l.CourseId == courseId,
                includes:
                [
                    l => l.Course
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            lessons = lessons
                .OrderBy(l => l.OrderNumber)
                .ThenBy(l => l.Id);

            ViewBag.Course = course;

            return View(lessons);
        }

        [HttpGet]
        public async Task<IActionResult> Create(
            int courseId,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == courseId &&
                     c.TrainerId == trainerId,
                tracked: false,
                cancellationToken: cancellationToken);

            if (course == null)
                return NotFound();

            var model = new LessonVM
            {
                CourseId = courseId
            };

            ViewBag.Course = course;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            LessonVM model,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == model.CourseId &&
                     c.TrainerId == trainerId,
                cancellationToken: cancellationToken);

            if (course == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Course = course;
                return View(model);
            }
            var titleExists = await _lessonRepository.GetOneAsync(
                l => l.CourseId == model.CourseId &&
                     l.Title.ToLower().Trim() ==
                     model.Title.ToLower().Trim(),
                tracked: false,
                cancellationToken: cancellationToken);

            if (titleExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    _localizer["DuplicateLessonTitle"]);

                ViewBag.Course = course;
                return View(model);
            }

            var orderExists = await _lessonRepository.GetOneAsync(
                l => l.CourseId == model.CourseId &&
                     l.OrderNumber == model.OrderNumber,
                tracked: false,
                cancellationToken: cancellationToken);

            if (orderExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.OrderNumber),
                    _localizer["DuplicateLessonOrderNumber"]);

                ViewBag.Course = course;
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

            await _lessonRepository.AddAsync(
                lesson,
                cancellationToken);

            if (await _lessonRepository.CommitAsync(
                cancellationToken) > 0)
            {
                TempData["Success"] =
                    _localizer["LessonCreatedSuccess"].ToString();

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        courseId = model.CourseId
                    });
            }

            TempData["Error"] =
                _localizer["LessonCreateFailed"].ToString();

            ViewBag.Course = course;

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(
            int id,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == id,
                includes:
                [
                    l => l.Course
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            if (lesson == null)
                return NotFound();

            if (lesson.Course.TrainerId != trainerId)
                return NotFound();

            var model = new LessonVM
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                VideoUrl = lesson.VideoUrl,
                OrderNumber = lesson.OrderNumber,
                CourseId = lesson.CourseId
            };

            ViewBag.Course = lesson.Course;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            LessonVM model,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == model.CourseId &&
                     c.TrainerId == trainerId,
                cancellationToken: cancellationToken);

            if (course == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Course = course;
                return View(model);
            }
            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == model.Id &&
                     l.CourseId == model.CourseId,
                cancellationToken: cancellationToken);

            if (lesson == null)
                return NotFound();

            var titleExists = await _lessonRepository.GetOneAsync(
                l => l.CourseId == model.CourseId &&
                     l.Title.ToLower().Trim() ==
                     model.Title.ToLower().Trim() &&
                     l.Id != model.Id,
                tracked: false,
                cancellationToken: cancellationToken);

            if (titleExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    _localizer["DuplicateLessonTitle"]);

                ViewBag.Course = course;
                return View(model);
            }

            var orderExists = await _lessonRepository.GetOneAsync(
                l => l.CourseId == model.CourseId &&
                     l.OrderNumber == model.OrderNumber &&
                     l.Id != model.Id,
                tracked: false,
                cancellationToken: cancellationToken);

            if (orderExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.OrderNumber),
                    _localizer["DuplicateLessonOrderNumber"]);

                ViewBag.Course = course;
                return View(model);
            }

            lesson.Title = model.Title.Trim();
            lesson.Description = model.Description?.Trim();
            lesson.VideoUrl = model.VideoUrl.Trim();
            lesson.OrderNumber = model.OrderNumber;

            _lessonRepository.Update(lesson);

            if (await _lessonRepository.CommitAsync(
                cancellationToken) > 0)
            {
                TempData["Success"] =
                    _localizer["LessonUpdatedSuccess"].ToString();

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        courseId = model.CourseId
                    });
            }

            TempData["Error"] =
                _localizer["LessonUpdateFailed"].ToString();

            ViewBag.Course = course;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == id,
                includes:
                [
                    l => l.Course,
                    l => l.CourseMaterials,
                    l => l.UserProgresses
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            if (lesson == null)
                return NotFound();

            // Security check
            if (lesson.Course.TrainerId != trainerId)
                return NotFound();

            return View(lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == id,
                includes:
                [
                    l => l.Course,
                    l => l.CourseMaterials,
                    l => l.UserProgresses
                ],
                tracked: true,
                cancellationToken: cancellationToken);

            if (lesson == null)
                return NotFound();
            if (lesson.Course.TrainerId != trainerId)
                return NotFound();

            int courseId = lesson.CourseId;

            int materialsCount =
                lesson.CourseMaterials?.Count ?? 0;

            int progressCount =
                lesson.UserProgresses?.Count ?? 0;

            _lessonRepository.Delete(lesson);

            if (await _lessonRepository.CommitAsync(
                cancellationToken) > 0)
            {
                if (materialsCount > 0 || progressCount > 0)
                {
                    TempData["Success"] =
                        string.Format(
                            _localizer["LessonDeletedSuccessWithRelatedData"].ToString(),
                            materialsCount,
                            progressCount);
                }
                else
                {
                    TempData["Success"] =
                        _localizer["LessonDeletedSuccess"].ToString();
                }
            }
            else
            {
                TempData["Error"] =
                    _localizer["LessonDeleteFailed"].ToString();
            }

            return RedirectToAction(
                nameof(Index),
                new
                {
                    courseId
                });
        }

        private int GetCurrentTrainerId()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

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