using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training_Platform.Models;
using Training_Platform.Repositories;

namespace Training_Platform.Areas.Trainer.Controllers
{
    [Area(SD.Trainer_Area)]
    
    public class LessonsController : Controller
    {
        private readonly IRepository<Lesson> _lessonRepository;
        private readonly IRepository<Course> _courseRepository;

        public LessonsController(
            IRepository<Lesson> lessonRepository,
            IRepository<Course> courseRepository)
        {
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int courseId)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == courseId &&
                     c.TrainerId == trainerId,
                tracked: false);

            if (course == null)
                return NotFound();

            var lessons = await _lessonRepository.GetAsync(
                l => l.CourseId == courseId,
                includes:
                [
                    l => l.Course
                ],
                tracked: false);

            lessons = lessons
                .OrderBy(l => l.OrderNumber)
                .ThenBy(l => l.Id);

            ViewBag.Course = course;

            return View(lessons);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == courseId &&
                     c.TrainerId == trainerId,
                tracked: false);

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
        public async Task<IActionResult> Create(LessonVM model)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == model.CourseId &&
                     c.TrainerId == trainerId);

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
                     model.Title.ToLower().Trim());

            if (titleExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "You already have a lesson with this title in this course.");

                ViewBag.Course = course;
                return View(model);
            }

            var orderExists = await _lessonRepository.GetOneAsync(
                l => l.CourseId == model.CourseId &&
                     l.OrderNumber == model.OrderNumber);

            if (orderExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.OrderNumber),
                    "This order number is already used in this course.");

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

            await _lessonRepository.AddAsync(lesson);

            if (await _lessonRepository.CommitAsync() > 0)
            {
                TempData["Success"] = "Lesson created successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new { courseId = model.CourseId });
            }

            TempData["Error"] = "Something went wrong while creating the lesson.";

            ViewBag.Course = course;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
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
                tracked: false);

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
        public async Task<IActionResult> Edit(LessonVM model)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == model.CourseId &&
                     c.TrainerId == trainerId);

            if (course == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Course = course;
                return View(model);
            }

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == model.Id &&
                     l.CourseId == model.CourseId);

            if (lesson == null)
                return NotFound();

            var titleExists = await _lessonRepository.GetOneAsync(
                l => l.CourseId == model.CourseId &&
                     l.Title.ToLower().Trim() ==
                     model.Title.ToLower().Trim() &&
                     l.Id != model.Id);

            if (titleExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "You already have a lesson with this title in this course.");

                ViewBag.Course = course;
                return View(model);
            }

            var orderExists = await _lessonRepository.GetOneAsync(
                l => l.CourseId == model.CourseId &&
                     l.OrderNumber == model.OrderNumber &&
                     l.Id != model.Id);

            if (orderExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.OrderNumber),
                    "This order number is already used in this course.");

                ViewBag.Course = course;
                return View(model);
            }

            lesson.Title = model.Title.Trim();
            lesson.Description = model.Description?.Trim();
            lesson.VideoUrl = model.VideoUrl.Trim();
            lesson.OrderNumber = model.OrderNumber;

            _lessonRepository.Update(lesson);

            if (await _lessonRepository.CommitAsync() > 0)
            {
                TempData["Success"] = "Lesson updated successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new { courseId = model.CourseId });
            }

            TempData["Error"] = "Something went wrong while updating the lesson.";

            ViewBag.Course = course;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
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
                tracked: false);

            if (lesson == null)
                return NotFound();

            if (lesson.Course.TrainerId != trainerId)
                return NotFound();

            return View(lesson);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int courseId)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();
            var course = await _courseRepository.GetOneAsync(
                c => c.Id == courseId &&
                     c.TrainerId == trainerId);

            if (course == null)
                return NotFound();

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == id &&
                     l.CourseId == courseId,
                includes:
                [
                    l => l.CourseMaterials,
                    l => l.UserProgresses
                ]);

            if (lesson == null)
                return NotFound();

            int materialsCount = lesson.CourseMaterials?.Count ?? 0;
            int progressCount = lesson.UserProgresses?.Count ?? 0;

            _lessonRepository.Delete(lesson);

            if (await _lessonRepository.CommitAsync() > 0)
            {
                if (materialsCount > 0 || progressCount > 0)
                {
                    TempData["Success"] =
                        $"Lesson deleted successfully. " +
                        $"{materialsCount} material(s) and " +
                        $"{progressCount} progress record(s) were also deleted.";
                }
                else
                {
                    TempData["Success"] =
                        "Lesson deleted successfully.";
                }
            }
            else
            {
                TempData["Error"] =
                    "Something went wrong while deleting the lesson.";
            }

            return RedirectToAction(
                nameof(Index),
                new { courseId });
        }
        private int GetCurrentTrainerId()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            return int.TryParse(userId, out int trainerId)
                ? trainerId
                : 0;
        }
    }
}