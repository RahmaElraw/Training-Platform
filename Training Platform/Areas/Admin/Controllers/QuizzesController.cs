using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    [Authorize(Roles = $"{RoleNames.SUPER_ADMIN}")]

    public class QuizzesController : Controller
    {
        private readonly IRepository<Quiz> _quizRepository;
        private readonly IRepository<Course> _courseRepository;

        private const int PageSize = 6;

        public QuizzesController(
            IRepository<Quiz> quizRepository,
            IRepository<Course> courseRepository)
        {
            _quizRepository = quizRepository;
            _courseRepository = courseRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(
            string? query,
            int page = 1)
        {
            var quizzes =
                await _quizRepository.GetAsync(
                    includes:
                    [
                        q => q.Course,
                        q => q.Questions,
                        q => q.QuizResults
                    ],
                    tracked: false);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();

                quizzes = quizzes.Where(q =>
                    q.Title.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    (q.Course != null &&
                     q.Course.Title.Contains(
                         query,
                         StringComparison.OrdinalIgnoreCase)));
            }

            quizzes = quizzes
                .OrderBy(q => q.Course.Title)
                .ThenBy(q => q.Title);

            int totalCount = quizzes.Count();

            var model = new QuizWithRelatedVM
            {
                Quizzes = quizzes
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
            await LoadQuizData();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuizVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadQuizData(model);
                return View(model);
            }

            var course =
                await _courseRepository.GetOneAsync(
                    c => c.Id == model.CourseId);

            if (course == null)
            {
                ModelState.AddModelError(
                    nameof(model.CourseId),
                    "Selected course does not exist.");

                await LoadQuizData(model);
                return View(model);
            }


            var exists =
                await _quizRepository.GetOneAsync(
                    q => q.CourseId == model.CourseId
                         &&
                         q.Title.ToLower().Trim()
                         ==
                         model.Title.ToLower().Trim());

            if (exists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "A quiz with this title already exists in this course.");

                await LoadQuizData(model);
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
                TempData["Success"] =
                    "Quiz created successfully.";

                return RedirectToAction(nameof(Index));
            }


            TempData["Error"] =
                "Something went wrong.";

            await LoadQuizData(model);

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var quiz =
                await _quizRepository.GetOneAsync(
                    q => q.Id == id);

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


            await LoadQuizData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuizVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadQuizData(model);
                return View(model);
            }


            var quiz =
                await _quizRepository.GetOneAsync(
                    q => q.Id == model.Id);

            if (quiz == null)
                return NotFound();

            var course =
                await _courseRepository.GetOneAsync(
                    c => c.Id == model.CourseId);

            if (course == null)
            {
                ModelState.AddModelError(
                    nameof(model.CourseId),
                    "Selected course does not exist.");

                await LoadQuizData(model);
                return View(model);
            }

            var exists =
                await _quizRepository.GetOneAsync(
                    q => q.CourseId == model.CourseId
                         &&
                         q.Title.ToLower().Trim()
                         ==
                         model.Title.ToLower().Trim()
                         &&
                         q.Id != model.Id);

            if (exists != null)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "A quiz with this title already exists in this course.");

                await LoadQuizData(model);
                return View(model);
            }


            quiz.Title =
                model.Title.Trim();

            quiz.PassingScore =
                model.PassingScore;

            quiz.TimeLimit =
                model.TimeLimit;

            quiz.CourseId =
                model.CourseId;


            _quizRepository.Update(quiz);


            if (await _quizRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Quiz updated successfully.";

                return RedirectToAction(nameof(Index));
            }


            TempData["Error"] =
                "Something went wrong.";

            await LoadQuizData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == id);

            if (quiz == null)
                return NotFound();

            _quizRepository.Delete(quiz);

            int result = await _quizRepository.CommitAsync();

            if (result > 0)
            {
                TempData["Success"] =
                    "Quiz and all related questions, options, and results deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Something went wrong while deleting the quiz.";
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var quiz =
                await _quizRepository.GetOneAsync(
                    q => q.Id == id,
                    includes:
                    [
                        q => q.Course,
                        q => q.Questions,
                        q => q.QuizResults
                    ]);

            if (quiz == null)
                return NotFound();


            return View(quiz);
        }
        private async Task LoadQuizData(
            QuizVM? model = null)
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
    }
}
