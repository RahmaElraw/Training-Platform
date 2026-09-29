using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Training_Platform.Models;
using Training_Platform.Repositories.IRepositories;
using Training_Platform.ViewModels;

namespace Training_Platform.Areas.Trainer.Controllers
{
    [Area(SD.Trainer_Area)]
    public class MaterialsController : Controller
    {
        private readonly IRepository<CourseMaterial> _materialRepository;
        private readonly IRepository<Lesson> _lessonRepository;

        public MaterialsController(
            IRepository<CourseMaterial> materialRepository,
            IRepository<Lesson> lessonRepository)
        {
            _materialRepository = materialRepository;
            _lessonRepository = lessonRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int lessonId,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == lessonId &&
                     l.Course.TrainerId == trainerId,
                includes: new Expression<Func<Lesson, object>>[]
                {
                    l => l.Course
                },
                tracked: false,
                cancellationToken: cancellationToken);

            if (lesson == null)
                return NotFound();

            var materials = await _materialRepository.GetAsync(
                m => m.LessonId == lessonId,
                tracked: false,
                cancellationToken: cancellationToken);

            var vm = materials.Select(m => new CourseMaterialVM
            {
                Id = m.Id,
                Title = m.Title ?? string.Empty,
                Url = m.Url,
                LessonId = m.LessonId
            });

            ViewBag.LessonId = lesson.Id;
            ViewBag.LessonTitle = lesson.Title;

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create(
            int lessonId,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == lessonId &&
                     l.Course.TrainerId == trainerId,
                includes: new Expression<Func<Lesson, object>>[]
                {
                    l => l.Course
                },
                tracked: false,
                cancellationToken: cancellationToken);

            if (lesson == null)
                return NotFound();

            var vm = new CourseMaterialVM
            {
                LessonId = lesson.Id
            };

            ViewBag.LessonTitle = lesson.Title;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CourseMaterialVM vm,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == vm.LessonId &&
                     l.Course.TrainerId == trainerId,
                includes: new Expression<Func<Lesson, object>>[]
                {
                    l => l.Course
                },
                tracked: false,
                cancellationToken: cancellationToken);

            if (lesson == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.LessonTitle = lesson.Title;
                return View(vm);
            }

            var material = new CourseMaterial
            {
                Title = vm.Title,
                Url = vm.Url,
                LessonId = vm.LessonId
            };

            await _materialRepository.AddAsync(
                material,
                cancellationToken);

            await _materialRepository.CommitAsync(
                cancellationToken);

            return RedirectToAction(
                nameof(Index),
                new { lessonId = vm.LessonId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var material = await _materialRepository.GetOneAsync(
                m => m.Id == id,
                includes: new Expression<Func<CourseMaterial, object>>[]
                {
                    m => m.Lesson
                },
                tracked: false,
                cancellationToken: cancellationToken);

            if (material == null)
                return NotFound();

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == material.LessonId &&
                     l.Course.TrainerId == trainerId,
                includes: new Expression<Func<Lesson, object>>[]
                {
                    l => l.Course
                },
                tracked: false,
                cancellationToken: cancellationToken);

            if (lesson == null)
                return NotFound();

            var vm = new CourseMaterialVM
            {
                Id = material.Id,
                Title = material.Title ?? string.Empty,
                Url = material.Url,
                LessonId = material.LessonId
            };

            ViewBag.LessonTitle = lesson.Title;

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            CourseMaterialVM vm,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == vm.LessonId &&
                     l.Course.TrainerId == trainerId,
                includes: new Expression<Func<Lesson, object>>[]
                {
                    l => l.Course
                },
                tracked: false,
                cancellationToken: cancellationToken);

            if (lesson == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.LessonTitle = lesson.Title;
                return View(vm);
            }

            var material = await _materialRepository.GetOneAsync(
                m => m.Id == vm.Id &&
                     m.LessonId == vm.LessonId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (material == null)
                return NotFound();

            material.Title = vm.Title;
            material.Url = vm.Url;

            _materialRepository.Update(material);

            await _materialRepository.CommitAsync(
                cancellationToken);

            return RedirectToAction(
                nameof(Index),
                new { lessonId = vm.LessonId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            int lessonId,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == lessonId &&
                     l.Course.TrainerId == trainerId,
                includes: new Expression<Func<Lesson, object>>[]
                {
                    l => l.Course
                },
                tracked: false,
                cancellationToken: cancellationToken);

            if (lesson == null)
                return NotFound();

            var material = await _materialRepository.GetOneAsync(
                m => m.Id == id &&
                     m.LessonId == lessonId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (material == null)
                return NotFound();

            _materialRepository.Delete(material);

            await _materialRepository.CommitAsync(
                cancellationToken);

            return RedirectToAction(
                nameof(Index),
                new { lessonId });
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
    }
}