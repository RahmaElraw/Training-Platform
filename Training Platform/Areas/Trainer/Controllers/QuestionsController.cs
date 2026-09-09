using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Training_Platform.Models;
using Training_Platform.Repositories;
using Training_Platform.ViewModels;

namespace Training_Platform.Areas.Trainer.Controllers
{
    [Area(SD.Trainer_Area)]
    public class QuestionsController : Controller
    {
        private readonly IRepository<Question> _questionRepository;
        private readonly IRepository<QuestionOption> _questionOptionRepository;
        private readonly IRepository<Quiz> _quizRepository;

        public QuestionsController(
            IRepository<Question> questionRepository,
            IRepository<QuestionOption> questionOptionRepository,
            IRepository<Quiz> quizRepository)
        {
            _questionRepository = questionRepository;
            _questionOptionRepository = questionOptionRepository;
            _quizRepository = quizRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int quizId)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == quizId &&
                     q.Course.TrainerId == trainerId,
                includes: [q => q.Course],
                tracked: false);

            if (quiz == null)
                return NotFound();

            var questions = await _questionRepository.GetAsync(
                q => q.QuizId == quizId,
                tracked: false);

            questions = questions
                .OrderBy(q => q.Id);

            ViewBag.Quiz = quiz;

            return View(questions);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int quizId)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == quizId &&
                     q.Course.TrainerId == trainerId,
                includes: [q => q.Course],
                tracked: false);

            if (quiz == null)
                return NotFound();

            var model = new QuestionVM
            {
                QuizId = quizId,
                QuestionType = QuestionType.MultipleChoice
            };

            ViewBag.Quiz = quiz;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionVM model)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == model.QuizId &&
                     q.Course.TrainerId == trainerId,
                includes: [q => q.Course]);

            if (quiz == null)
                return NotFound();

            var options = model.QuestionOptions?
                .Where(o => !string.IsNullOrWhiteSpace(o.OptionText))
                .ToList()
                ?? new List<QuestionOptionVM>();

            ValidateOptions(model.QuestionType, options);

            if (!ModelState.IsValid)
            {
                ViewBag.Quiz = quiz;
                return View(model);
            }

            var questionExists = await _questionRepository.GetOneAsync(
                q => q.QuizId == model.QuizId &&
                     q.QuestionText.ToLower().Trim()
                     == model.QuestionText.ToLower().Trim());

            if (questionExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.QuestionText),
                    "You already have a question with this text in this quiz.");

                ViewBag.Quiz = quiz;
                return View(model);
            }

            var question = new Question
            {
                QuestionText = model.QuestionText.Trim(),
                Mark = model.Mark,
                QuestionType = model.QuestionType,
                QuizId = model.QuizId
            };

            foreach (var option in options)
            {
                question.QuestionOptions.Add(new QuestionOption
                {
                    OptionText = option.OptionText!.Trim(),
                    IsCorrect = option.IsCorrect
                });
            }

            await _questionRepository.AddAsync(question);

            if (await _questionRepository.CommitAsync() > 0)
            {
                TempData["Success"] = "Question created successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new { quizId = model.QuizId });
            }

            TempData["Error"] =
                "Something went wrong while creating the question.";

            ViewBag.Quiz = quiz;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var question = await _questionRepository.GetOneAsync(
                q => q.Id == id &&
                     q.Quiz.Course.TrainerId == trainerId,
                includes:
                [
                    q => q.Quiz,
                    q => q.QuestionOptions
                ],
                tracked: false);

            if (question == null)
                return NotFound();

            var model = new QuestionVM
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                Mark = question.Mark,
                QuestionType = question.QuestionType,
                QuizId = question.QuizId,

                QuestionOptions = question.QuestionOptions
                    .Select(o => new QuestionOptionVM
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect,
                        QuestionId = o.QuestionId
                    })
                    .ToList()
            };

            ViewBag.Quiz = question.Quiz;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionVM model)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var question = await _questionRepository.GetOneAsync(
                q => q.Id == model.Id &&
                     q.QuizId == model.QuizId &&
                     q.Quiz.Course.TrainerId == trainerId,
                includes:
                [
                    q => q.Quiz,
                    q => q.QuestionOptions
                ]);

            if (question == null)
                return NotFound();

            var options = model.QuestionOptions?
                .Where(o => !string.IsNullOrWhiteSpace(o.OptionText))
                .ToList()
                ?? new List<QuestionOptionVM>();

            ValidateOptions(model.QuestionType, options);

            if (!ModelState.IsValid)
            {
                ViewBag.Quiz = question.Quiz;
                return View(model);
            }

            var questionExists = await _questionRepository.GetOneAsync(
                q => q.QuizId == model.QuizId &&
                     q.Id != model.Id &&
                     q.QuestionText.ToLower().Trim()
                     == model.QuestionText.ToLower().Trim());

            if (questionExists != null)
            {
                ModelState.AddModelError(
                    nameof(model.QuestionText),
                    "You already have a question with this text in this quiz.");

                ViewBag.Quiz = question.Quiz;

                return View(model);
            }

            question.QuestionText = model.QuestionText.Trim();
            question.Mark = model.Mark;
            question.QuestionType = model.QuestionType;

            foreach (var oldOption in question.QuestionOptions.ToList())
            {
                _questionOptionRepository.Delete(oldOption);
            }

            foreach (var option in options)
            {
                await _questionOptionRepository.AddAsync(
                    new QuestionOption
                    {
                        OptionText = option.OptionText!.Trim(),
                        IsCorrect = option.IsCorrect,
                        QuestionId = question.Id
                    });
            }

            if (await _questionRepository.CommitAsync() > 0)
            {
                TempData["Success"] = "Question updated successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new { quizId = model.QuizId });
            }

            TempData["Error"] =
                "Something went wrong while updating the question.";

            ViewBag.Quiz = question.Quiz;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var question = await _questionRepository.GetOneAsync(
                q => q.Id == id &&
                     q.Quiz.Course.TrainerId == trainerId,
                includes:
                [
                    q => q.Quiz,
                    q => q.QuestionOptions
                ],
                tracked: false);

            if (question == null)
                return NotFound();

            return View(question);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int quizId)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var question = await _questionRepository.GetOneAsync(
                q => q.Id == id &&
                     q.QuizId == quizId &&
                     q.Quiz.Course.TrainerId == trainerId);

            if (question == null)
                return NotFound();

            _questionRepository.Delete(question);

            if (await _questionRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Question and its options were deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Something went wrong while deleting the question.";
            }

            return RedirectToAction(
                nameof(Index),
                new { quizId });
        }

        private void ValidateOptions(
            QuestionType questionType,
            List<QuestionOptionVM> options)
        {
            if (questionType == QuestionType.TrueFalse)
            {
                if (options.Count != 2)
                {
                    ModelState.AddModelError(
                        nameof(QuestionVM.QuestionOptions),
                        "True/False questions must have exactly 2 options.");
                }

                if (options.Count(o => o.IsCorrect) != 1)
                {
                    ModelState.AddModelError(
                        nameof(QuestionVM.QuestionOptions),
                        "True/False questions must have exactly one correct option.");
                }
            }
            else
            {
                if (options.Count < 2)
                {
                    ModelState.AddModelError(
                        nameof(QuestionVM.QuestionOptions),
                        "Multiple choice questions must have at least 2 options.");
                }

                if (!options.Any(o => o.IsCorrect))
                {
                    ModelState.AddModelError(
                        nameof(QuestionVM.QuestionOptions),
                        "You must select at least one correct option.");
                }
            }
        }
        private int GetCurrentTrainerId()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            return int.TryParse(userId, out int trainerId)
                ? trainerId
                : 0;
        }
    }
}