using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Training_Platform.ViewModels.Trainee;
using QuestionVM = Training_Platform.ViewModels.Trainee.QuestionVM;

namespace Training_Platform.Areas.Trainee.Controllers
{
    [Area(SD.Trainee_Area)]
    [Authorize]
    public class QuizzesController : Controller
    {
        private readonly IRepository<Quiz> _quizRepository;
        private readonly IRepository<Question> _questionRepository;
        private readonly IRepository<QuizResult> _quizResultRepository;
        private readonly IRepository<Enrollment> _enrollmentRepository;

        public QuizzesController(
            IRepository<Quiz> quizRepository,
            IRepository<Question> questionRepository,
            IRepository<QuizResult> quizResultRepository,
            IRepository<Enrollment> enrollmentRepository)
        {
            _quizRepository = quizRepository;
            _questionRepository = questionRepository;
            _quizResultRepository = quizResultRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Take(int id, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var existingResult = await _quizResultRepository.GetOneAsync(
                r => r.UserId == userId && r.QuizId == id,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (existingResult != null)
            {
                return RedirectToAction(nameof(Result), new { id = existingResult.Id });
            }

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == id,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (quiz is null)
                return NotFound();

            var questions = await _questionRepository.GetAsync(
                q => q.QuizId == id,
                includes: [q => q.QuestionOptions],
                tracked: false,
                cancellationToken: cancellationToken
            );

            var vm = new QuizTakeVM
            {
                QuizId = quiz.Id,
                Title = quiz.Title,
                TimeLimitMinutes = quiz.TimeLimit,
                CourseId = quiz.CourseId,
                Questions = questions.Select(q => new QuestionVM
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    Mark = q.Mark,
                    QuestionType = q.QuestionType,
                    Options = q.QuestionOptions.Select(o => new OptionVM
                    {
                        OptionId = o.Id,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            QuizSubmitVM model,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            // Prevent multiple submissions
            var existingResult = await _quizResultRepository.GetOneAsync(
                r => r.UserId == userId && r.QuizId == model.QuizId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (existingResult != null)
            {
                return RedirectToAction(nameof(Result), new { id = existingResult.Id });
            }

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == model.QuizId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (quiz is null)
                return NotFound();

            var questions = await _questionRepository.GetAsync(
                q => q.QuizId == model.QuizId,
                includes: [q => q.QuestionOptions],
                tracked: false,
                cancellationToken: cancellationToken
            );

            int totalEarnedScore = 0;

            foreach (var question in questions)
            {
                var submittedAnswer = model.Answers?
                    .FirstOrDefault(a => a.QuestionId == question.Id);

                if (submittedAnswer != null)
                {
                    var correctOption = question.QuestionOptions
                        .FirstOrDefault(o => o.IsCorrect);

                    if (correctOption != null && correctOption.Id == submittedAnswer.SelectedOptionId)
                    {
                        totalEarnedScore += question.Mark;
                    }
                }
            }

            bool isPassed = totalEarnedScore >= quiz.PassingScore;

            var result = new QuizResult
            {
                QuizId = quiz.Id,
                UserId = userId,
                Score = totalEarnedScore,
                IsPassed = isPassed,
                SubmittedAt = DateTime.UtcNow
            };

            await _quizResultRepository.AddAsync(result, cancellationToken);
            await _quizResultRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Result), new { id = result.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Result(
            int id,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var result = await _quizResultRepository.GetOneAsync(
                r => r.Id == id && r.UserId == userId,
                includes: [r => r.Quiz, r => r.Quiz.Questions],
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (result is null)
                return NotFound();

            var totalPossible = result.Quiz.Questions?.Sum(q => q.Mark) ?? 0;

            var vm = new QuizResultVM
            {
                ResultId = result.Id,
                QuizId = result.QuizId,
                QuizTitle = result.Quiz.Title,
                Score = result.Score,
                TotalPossibleScore = totalPossible,
                PassingScore = result.Quiz.PassingScore,
                IsPassed = result.IsPassed,
                SubmittedAt = result.SubmittedAt,
                CourseId = result.Quiz.CourseId
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> MyQuizzes(CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var enrollments = await _enrollmentRepository.GetAsync(
                e => e.UserId == userId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToList();

            if (!enrolledCourseIds.Any())
            {
                return View(new List<TraineeQuizItemVM>());
            }

            var quizzes = await _quizRepository.GetAsync(
                q => enrolledCourseIds.Contains(q.CourseId),
                includes: [q => q.Course, q => q.Questions],
                tracked: false,
                cancellationToken: cancellationToken
            );

            var results = await _quizResultRepository.GetAsync(
                r => r.UserId == userId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            var resultMap = results
                .GroupBy(r => r.QuizId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        BestScore = g.Max(r => r.Score),
                        IsPassed = g.Any(r => r.IsPassed),
                        LastAttempt = g.OrderByDescending(r => r.SubmittedAt).FirstOrDefault()
                    }
                );

            var vmList = quizzes.Select(q =>
            {
                var hasResult = resultMap.TryGetValue(q.Id, out var res);
                var totalPossible = q.Questions?.Sum(quest => quest.Mark) ?? 0;

                return new TraineeQuizItemVM
                {
                    QuizId = q.Id,
                    QuizTitle = q.Title,
                    CourseId = q.CourseId,
                    CourseTitle = q.Course?.Title ?? "N/A",
                    TimeLimitMinutes = q.TimeLimit,
                    PassingScore = q.PassingScore,
                    QuestionCount = q.Questions?.Count ?? 0,
                    TotalPossibleScore = totalPossible,
                    HasAttempted = hasResult,
                    BestScore = hasResult ? res!.BestScore : null,
                    IsPassed = hasResult && res!.IsPassed,
                    LastAttemptDate = hasResult ? res!.LastAttempt?.SubmittedAt : null
                };
            })
            .OrderByDescending(q => q.QuizId)
            .ToList();

            return View(vmList);
        }

        private int GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var id))
                throw new UnauthorizedAccessException();
            return id;
        }
    }
}