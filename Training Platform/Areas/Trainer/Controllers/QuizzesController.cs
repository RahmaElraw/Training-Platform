using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Training_Platform.Models;
using Training_Platform.Repositories;
using Training_Platform.ViewModels;

namespace Training_Platform.Areas.Trainer.Controllers
{
    [Area(SD.Trainer_Area)]
    public class QuizzesController : Controller
    {
        private readonly IRepository<Quiz> _quizRepository;
        private readonly IRepository<Course> _courseRepository;

        public QuizzesController(
            IRepository<Quiz> quizRepository,
            IRepository<Course> courseRepository)
        {
            _quizRepository = quizRepository;
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

            var quizzes = await _quizRepository.GetAsync(
                q => q.CourseId == courseId,
                includes:
                [
                    q => q.Course
                ],
                tracked: false);

            quizzes = quizzes
                .OrderBy(q => q.Id);

            ViewBag.Course = course;

            return View(quizzes);
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

            var model = new QuizVM
            {
                CourseId = courseId
            };

            ViewBag.Course = course;

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuizVM model)
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

            var titleExists = await _quizRepository.GetOneAsync(
                q => q.CourseId == model.CourseId &&
                     q.Title.ToLower().Trim() ==
                     model.Title.ToLower().Trim());

            if (titleExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "You already have a quiz with this title in this course.");

                ViewBag.Course = course;
                return View(model);
            }

            var quiz = new Quiz
            {
                Title = model.Title.Trim(),
                PassingScore = model.PassingScore,
                TimeLimit = model.TimeLimit,
                CourseId = model.CourseId
            };

            await _quizRepository.AddAsync(quiz);

            if (await _quizRepository.CommitAsync() > 0)
            {
                TempData["Success"] = "Quiz created successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new { courseId = model.CourseId });
            }

            TempData["Error"] =
                "Something went wrong while creating the quiz.";

            ViewBag.Course = course;

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == id,
                includes:
                [
                    q => q.Course
                ],
                tracked: false);

            if (quiz == null)
                return NotFound();

            if (quiz.Course.TrainerId != trainerId)
                return NotFound();

            var model = new QuizVM
            {
                Id = quiz.Id,
                Title = quiz.Title,
                PassingScore = quiz.PassingScore,
                TimeLimit = quiz.TimeLimit,
                CourseId = quiz.CourseId
            };

            ViewBag.Course = quiz.Course;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuizVM model)
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
            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == model.Id &&
                     q.CourseId == model.CourseId);

            if (quiz == null)
                return NotFound();
            var titleExists = await _quizRepository.GetOneAsync(
                q => q.CourseId == model.CourseId &&
                     q.Title.ToLower().Trim() ==
                     model.Title.ToLower().Trim() &&
                     q.Id != model.Id);

            if (titleExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "You already have a quiz with this title in this course.");

                ViewBag.Course = course;
                return View(model);
            }

            quiz.Title = model.Title.Trim();
            quiz.PassingScore = model.PassingScore;
            quiz.TimeLimit = model.TimeLimit;

            _quizRepository.Update(quiz);

            if (await _quizRepository.CommitAsync() > 0)
            {
                TempData["Success"] = "Quiz updated successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new { courseId = model.CourseId });
            }

            TempData["Error"] =
                "Something went wrong while updating the quiz.";

            ViewBag.Course = course;

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == id,
                includes:
                [
                    q => q.Course,
                    q => q.Questions,
                    q => q.QuizResults
                ],
                tracked: false);

            if (quiz == null)
                return NotFound();
            if (quiz.Course.TrainerId != trainerId)
                return NotFound();

            return View(quiz);
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

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == id &&
                     q.CourseId == courseId,
                includes:
                [
                    q => q.Questions,
                    q => q.QuizResults
                ]);

            if (quiz == null)
                return NotFound();

            int questionsCount = quiz.Questions?.Count ?? 0;
            int resultsCount = quiz.QuizResults?.Count ?? 0;

            _quizRepository.Delete(quiz);

            if (await _quizRepository.CommitAsync() > 0)
            {
                if (questionsCount > 0 || resultsCount > 0)
                {
                    TempData["Success"] =
                        $"Quiz deleted successfully. " +
                        $"{questionsCount} question(s) and " +
                        $"{resultsCount} result(s) were also deleted.";
                }
                else
                {
                    TempData["Success"] =
                        "Quiz deleted successfully.";
                }
            }
            else
            {
                TempData["Error"] =
                    "Something went wrong while deleting the quiz.";
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