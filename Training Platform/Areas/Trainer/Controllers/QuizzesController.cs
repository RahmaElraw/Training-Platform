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

        // =====================================================
        // INDEX
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var quizzes = await _quizRepository.GetAsync(
                q => q.Course.TrainerId == trainerId,
                includes:
                [
                    q => q.Course
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            quizzes = quizzes
                .OrderBy(q => q.Course.Title)
                .ThenBy(q => q.Title);

            return View(quizzes);
        }


        // =====================================================
        // CREATE - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create(
            int? courseId,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            // Coming from Course Details
            if (courseId.HasValue)
            {
                var course = await _courseRepository.GetOneAsync(
                    c => c.Id == courseId.Value &&
                         c.TrainerId == trainerId,
                    tracked: false,
                    cancellationToken: cancellationToken);

                if (course == null)
                    return NotFound();

                ViewBag.Course = course;
                ViewBag.IsCoursePreselected = true;

                return View(new QuizVM
                {
                    CourseId = course.Id
                });
            }

            // Coming from Quizzes Index / Dashboard
            var courses = await _courseRepository.GetAsync(
                c => c.TrainerId == trainerId,
                tracked: false,
                cancellationToken: cancellationToken);

            ViewBag.Courses = courses
                .OrderBy(c => c.Title)
                .ToList();

            ViewBag.IsCoursePreselected = false;

            return View(new QuizVM());
        }


        // =====================================================
        // CREATE - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            QuizVM model,
            bool isCoursePreselected,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == model.CourseId &&
                     c.TrainerId == trainerId,
                tracked: false,
                cancellationToken: cancellationToken);

            if (course == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await PrepareCreateView(
                    model,
                    trainerId,
                    isCoursePreselected,
                    cancellationToken);

                return View(model);
            }

            string normalizedTitle =
                model.Title.Trim().ToLower();

            var existingQuiz = await _quizRepository.GetOneAsync(
                q => q.CourseId == model.CourseId &&
                     q.Title.ToLower() == normalizedTitle,
                tracked: false,
                cancellationToken: cancellationToken);

            if (existingQuiz != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "A quiz with this title already exists in this course.");

                await PrepareCreateView(
                    model,
                    trainerId,
                    isCoursePreselected,
                    cancellationToken);

                return View(model);
            }

            var quiz = new Quiz
            {
                Title = model.Title.Trim(),
                PassingScore = model.PassingScore,
                TimeLimit = model.TimeLimit,
                CourseId = model.CourseId
            };

            await _quizRepository.AddAsync(
                quiz,
                cancellationToken);

            if (await _quizRepository.CommitAsync(
                    cancellationToken) > 0)
            {
                TempData["Success"] =
                    "Quiz created successfully.";

                return RedirectToAction(
                    nameof(Index));
            }

            TempData["Error"] =
                "Something went wrong while creating the quiz.";

            await PrepareCreateView(
                model,
                trainerId,
                isCoursePreselected,
                cancellationToken);

            return View(model);
        }


        // =====================================================
        // DETAILS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == id &&
                     q.Course.TrainerId == trainerId,
                includes:
                [
                    q => q.Course,
                    q => q.Questions,
                    q => q.QuizResults
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            if (quiz == null)
                return NotFound();

            return View(quiz);
        }


        // =====================================================
        // EDIT - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == id &&
                     q.Course.TrainerId == trainerId,
                includes:
                [
                    q => q.Course
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            if (quiz == null)
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


        // =====================================================
        // EDIT - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            QuizVM model,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == model.Id &&
                     q.Course.TrainerId == trainerId,
                includes:
                [
                    q => q.Course
                ],
                cancellationToken: cancellationToken);

            if (quiz == null)
                return NotFound();

            // Course cannot be changed from Edit
            model.CourseId = quiz.CourseId;

            if (!ModelState.IsValid)
            {
                ViewBag.Course = quiz.Course;
                return View(model);
            }

            string normalizedTitle =
                model.Title.Trim().ToLower();

            var existingQuiz = await _quizRepository.GetOneAsync(
                q => q.CourseId == quiz.CourseId &&
                     q.Id != quiz.Id &&
                     q.Title.ToLower() == normalizedTitle,
                tracked: false,
                cancellationToken: cancellationToken);

            if (existingQuiz != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "A quiz with this title already exists in this course.");

                ViewBag.Course = quiz.Course;

                return View(model);
            }

            quiz.Title = model.Title.Trim();
            quiz.PassingScore = model.PassingScore;
            quiz.TimeLimit = model.TimeLimit;

            _quizRepository.Update(quiz);

            if (await _quizRepository.CommitAsync(
                    cancellationToken) > 0)
            {
                TempData["Success"] =
                    "Quiz updated successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = quiz.Id });
            }

            TempData["Error"] =
                "Something went wrong while updating the quiz.";

            ViewBag.Course = quiz.Course;

            return View(model);
        }


        // =====================================================
        // DELETE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == id &&
                     q.Course.TrainerId == trainerId,
                cancellationToken: cancellationToken);

            if (quiz == null)
                return NotFound();

            int courseId = quiz.CourseId;

            _quizRepository.Delete(quiz);

            if (await _quizRepository.CommitAsync(
                    cancellationToken) > 0)
            {
                TempData["Success"] =
                    "Quiz and its related data were deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Something went wrong while deleting the quiz.";
            }

            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // PREPARE CREATE VIEW
        // =====================================================

        private async Task PrepareCreateView(
            QuizVM model,
            int trainerId,
            bool isCoursePreselected,
            CancellationToken cancellationToken)
        {
            ViewBag.IsCoursePreselected =
                isCoursePreselected;

            if (isCoursePreselected)
            {
                var course = await _courseRepository.GetOneAsync(
                    c => c.Id == model.CourseId &&
                         c.TrainerId == trainerId,
                    tracked: false,
                    cancellationToken: cancellationToken);

                ViewBag.Course = course;
            }
            else
            {
                var courses = await _courseRepository.GetAsync(
                    c => c.TrainerId == trainerId,
                    tracked: false,
                    cancellationToken: cancellationToken);

                ViewBag.Courses = courses
                    .OrderBy(c => c.Title)
                    .ToList();
            }
        }


        // =====================================================
        // CURRENT TRAINER ID
        // =====================================================

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