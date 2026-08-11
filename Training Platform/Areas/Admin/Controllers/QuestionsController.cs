using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    public class QuestionsController : Controller
    {
        private readonly IRepository<Question> _questionRepository;
        private readonly IRepository<Quiz> _quizRepository;
        private readonly IRepository<QuestionOption> _questionOptionRepository;

        private const int PageSize = 6;

        public QuestionsController(
            IRepository<Question> questionRepository,
            IRepository<Quiz> quizRepository,
            IRepository<QuestionOption> questionOptionRepository)
        {
            _questionRepository = questionRepository;
            _quizRepository = quizRepository;
            _questionOptionRepository = questionOptionRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(
            string? query,
            int page = 1)
        {
            if (page < 1)
                page = 1;

            var questions = await _questionRepository.GetAsync(
                includes:
                [
                    q => q.Quiz,
                    q => q.QuestionOptions
                ],
                tracked: false);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();

                questions = questions.Where(q =>
                    q.QuestionText.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    (q.Quiz != null &&
                     q.Quiz.Title.Contains(
                         query,
                         StringComparison.OrdinalIgnoreCase)));
            }

            questions = questions
                .OrderBy(q => q.Quiz != null ? q.Quiz.Title : "")
                .ThenBy(q => q.Id);

            int totalCount = questions.Count();

            int totalPages = (int)Math.Ceiling(
                totalCount / (double)PageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var model = new QuestionWithRelatedVM
            {
                Questions = questions
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize),

                CurrentPage = page,

                TotalPages = totalPages,

                Query = query
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new QuestionVM
            {
                QuestionOptions = new List<QuestionOptionVM>
                {
                    new QuestionOptionVM(),
                    new QuestionOptionVM(),
                    new QuestionOptionVM(),
                    new QuestionOptionVM()
                }
            };

            await LoadQuestionData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionVM model)
        {
            model.QuestionOptions ??=
                new List<QuestionOptionVM>();

            if (!ModelState.IsValid)
            {
                return await ReturnCreateView(model);
            }
            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == model.QuizId);

            if (quiz == null)
            {
                ModelState.AddModelError(
                    nameof(model.QuizId),
                    "Selected quiz does not exist.");

                return await ReturnCreateView(model);
            }
            var options = model.QuestionOptions
                .Where(o =>
                    !string.IsNullOrWhiteSpace(o.OptionText))
                .ToList();

            if (!options.Any())
            {
                ModelState.AddModelError(
                    nameof(model.QuestionOptions),
                    "Please add at least one option.");

                return await ReturnCreateView(model);
            }
            foreach (var option in options)
            {
                option.OptionText =
                    option.OptionText.Trim();
            }
            bool duplicateOptions =
                options
                    .GroupBy(o =>
                        o.OptionText.Trim(),
                        StringComparer.OrdinalIgnoreCase)
                    .Any(g => g.Count() > 1);

            if (duplicateOptions)
            {
                ModelState.AddModelError(
                    nameof(model.QuestionOptions),
                    "Question options cannot be duplicated.");

                return await ReturnCreateView(model);
            }
            int correctAnswers =
                options.Count(o => o.IsCorrect);


            if (correctAnswers == 0)
            {
                ModelState.AddModelError(
                    nameof(model.QuestionOptions),
                    "Please select one correct answer.");

                return await ReturnCreateView(model);
            }
            if (model.QuestionType ==
                QuestionType.MultipleChoice)
            {
                if (options.Count < 2)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionOptions),
                        "Multiple choice questions must have at least two options.");

                    return await ReturnCreateView(model);
                }

                if (correctAnswers > 1)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionOptions),
                        "Multiple choice questions can have only one correct answer.");

                    return await ReturnCreateView(model);
                }
            }

            if (model.QuestionType ==
                QuestionType.TrueFalse)
            {
                if (options.Count != 2)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionOptions),
                        "True/False questions must have exactly two options.");

                    return await ReturnCreateView(model);
                }

                bool validTrueFalse =
                    options.All(o =>
                        o.OptionText.Equals(
                            "True",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        o.OptionText.Equals(
                            "False",
                            StringComparison.OrdinalIgnoreCase));


                if (!validTrueFalse)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionOptions),
                        "True/False questions can only have True and False options.");

                    return await ReturnCreateView(model);
                }

                bool hasTrue =
                    options.Any(o =>
                        o.OptionText.Equals(
                            "True",
                            StringComparison.OrdinalIgnoreCase));

                bool hasFalse =
                    options.Any(o =>
                        o.OptionText.Equals(
                            "False",
                            StringComparison.OrdinalIgnoreCase));


                if (!hasTrue || !hasFalse)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionOptions),
                        "True/False questions must contain both True and False.");

                    return await ReturnCreateView(model);
                }
                if (correctAnswers != 1)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionOptions),
                        "True/False questions must have exactly one correct answer.");

                    return await ReturnCreateView(model);
                }
            }
            var question = new Question
            {
                QuestionText =
                    model.QuestionText.Trim(),

                Mark =
                    model.Mark,

                QuestionType =
                    model.QuestionType,

                QuizId =
                    model.QuizId
            };


            await _questionRepository.AddAsync(question);

            int questionResult =
                await _questionRepository.CommitAsync();


            if (questionResult <= 0)
            {
                TempData["Error"] =
                    "Something went wrong while creating the question.";

                return await ReturnCreateView(model);
            }
            foreach (var item in options)
            {
                var option = new QuestionOption
                {
                    OptionText =
                        item.OptionText.Trim(),

                    IsCorrect =
                        item.IsCorrect,

                    QuestionId =
                        question.Id
                };

                await _questionOptionRepository.AddAsync(option);
            }


            int optionResult =
                await _questionOptionRepository.CommitAsync();


            if (optionResult <= 0)
            {
                TempData["Error"] =
                    "Question was created, but something went wrong while saving the options.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id = question.Id });
            }


            TempData["Success"] =
                "Question and options created successfully.";


            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var question =
                await _questionRepository.GetOneAsync(
                    q => q.Id == id,
                    includes:
                    [
                        q => q.QuestionOptions
                    ]);

            if (question == null)
                return NotFound();


            var model = new QuestionVM
            {
                Id =
                    question.Id,

                QuestionText =
                    question.QuestionText,

                Mark =
                    question.Mark,

                QuestionType =
                    question.QuestionType,

                QuizId =
                    question.QuizId,

                QuestionOptions =
                    question.QuestionOptions
                        .Select(o => new QuestionOptionVM
                        {
                            Id = o.Id,

                            OptionText =
                                o.OptionText,

                            IsCorrect =
                                o.IsCorrect,

                            QuestionId =
                                o.QuestionId
                        })
                        .ToList()
            };


            await LoadQuestionData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionVM model)
        {
            // Make sure the collection is never null
            model.QuestionOptions ??= new List<QuestionOptionVM>();


            if (!ModelState.IsValid)
            {
                await LoadQuestionOptions(model);
                await LoadQuestionData(model);

                return View(model);
            }

            var question = await _questionRepository.GetOneAsync(
                q => q.Id == model.Id,
                includes:
                [
                    q => q.QuestionOptions
                ]);

            if (question == null)
                return NotFound();

            var quiz = await _quizRepository.GetOneAsync(
                q => q.Id == model.QuizId);

            if (quiz == null)
            {
                ModelState.AddModelError(
                    nameof(model.QuizId),
                    "Selected quiz does not exist.");

                await LoadQuestionOptions(model);
                await LoadQuestionData(model);

                return View(model);
            }

            var options =
                question.QuestionOptions?.ToList()
                ?? new List<QuestionOption>();

            if (!options.Any())
            {
                ModelState.AddModelError(
                    nameof(model.QuestionType),
                    "Question must have at least one option.");

                await LoadQuestionOptions(model);
                await LoadQuestionData(model);

                return View(model);
            }
            bool duplicateOptions =
                options
                    .GroupBy(
                        o => o.OptionText.Trim(),
                        StringComparer.OrdinalIgnoreCase)
                    .Any(g => g.Count() > 1);

            if (duplicateOptions)
            {
                ModelState.AddModelError(
                    nameof(model.QuestionType),
                    "Question options cannot be duplicated.");

                await LoadQuestionOptions(model);
                await LoadQuestionData(model);

                return View(model);
            }
            int correctAnswers =
                options.Count(o => o.IsCorrect);

            if (model.QuestionType == QuestionType.TrueFalse)
            {
                if (options.Count != 2)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionType),
                        "True/False questions must have exactly two options.");

                    await LoadQuestionOptions(model);
                    await LoadQuestionData(model);

                    return View(model);
                }
                bool validTrueFalse =
                    options.All(o =>
                        o.OptionText.Trim().Equals(
                            "True",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        o.OptionText.Trim().Equals(
                            "False",
                            StringComparison.OrdinalIgnoreCase));

                if (!validTrueFalse)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionType),
                        "True/False questions can only contain True and False options.");

                    await LoadQuestionOptions(model);
                    await LoadQuestionData(model);

                    return View(model);
                }
                bool hasTrue =
                    options.Any(o =>
                        o.OptionText.Trim().Equals(
                            "True",
                            StringComparison.OrdinalIgnoreCase));

                bool hasFalse =
                    options.Any(o =>
                        o.OptionText.Trim().Equals(
                            "False",
                            StringComparison.OrdinalIgnoreCase));


                if (!hasTrue || !hasFalse)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionType),
                        "True/False questions must contain both True and False.");

                    await LoadQuestionOptions(model);
                    await LoadQuestionData(model);

                    return View(model);
                }
                if (correctAnswers != 1)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionType),
                        "True/False questions must have exactly one correct answer.");

                    await LoadQuestionOptions(model);
                    await LoadQuestionData(model);

                    return View(model);
                }
            }
            if (model.QuestionType == QuestionType.MultipleChoice)
            {
                if (options.Count < 2)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionType),
                        "Multiple choice questions must have at least two options.");

                    await LoadQuestionOptions(model);
                    await LoadQuestionData(model);

                    return View(model);
                }
                if (correctAnswers != 1)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionType),
                        "Multiple choice questions must have exactly one correct answer.");

                    await LoadQuestionOptions(model);
                    await LoadQuestionData(model);

                    return View(model);
                }
            }
            question.QuestionText =
                model.QuestionText.Trim();

            question.Mark =
                model.Mark;

            question.QuestionType =
                model.QuestionType;

            question.QuizId =
                model.QuizId;


            _questionRepository.Update(question);

            int result =
                await _questionRepository.CommitAsync();


            if (result > 0)
            {
                TempData["Success"] =
                    "Question updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] =
                "Something went wrong while updating the question.";

            await LoadQuestionOptions(model);
            await LoadQuestionData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var question =
                await _questionRepository.GetOneAsync(
                    q => q.Id == id,
                    includes:
                    [
                        q => q.QuestionOptions
                    ]);


            if (question == null)
                return NotFound();


            if (question.QuestionOptions.Any())
            {
                TempData["Error"] =
                    "Cannot delete question because it has question options.";

                return RedirectToAction(nameof(Index));
            }


            _questionRepository.Delete(question);


            if (await _questionRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Question deleted successfully.";
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
            var question =
                await _questionRepository.GetOneAsync(
                    q => q.Id == id,
                    includes:
                    [
                        q => q.Quiz,
                        q => q.QuestionOptions
                    ]);


            if (question == null)
                return NotFound();


            return View(question);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOption(
            QuestionOptionVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    "Please enter valid option data.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id = model.QuestionId });
            }


            if (string.IsNullOrWhiteSpace(model.OptionText))
            {
                TempData["Error"] =
                    "Option text is required.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id = model.QuestionId });
            }


            var question =
                await _questionRepository.GetOneAsync(
                    q => q.Id == model.QuestionId,
                    includes:
                    [
                        q => q.QuestionOptions
                    ]);


            if (question == null)
                return NotFound();


            string optionText =
                model.OptionText.Trim();

            bool duplicateOption =
                question.QuestionOptions.Any(o =>
                    o.OptionText.Equals(
                        optionText,
                        StringComparison.OrdinalIgnoreCase));


            if (duplicateOption)
            {
                TempData["Error"] =
                    "This option already exists.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id = model.QuestionId });
            }
            if (question.QuestionType ==
                QuestionType.TrueFalse)
            {
                if (question.QuestionOptions.Count >= 2)
                {
                    TempData["Error"] =
                        "True/False questions can only have two options.";

                    return RedirectToAction(
                        nameof(Edit),
                        new { id = model.QuestionId });
                }


                bool validTrueFalse =
                    optionText.Equals(
                        "True",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    optionText.Equals(
                        "False",
                        StringComparison.OrdinalIgnoreCase);


                if (!validTrueFalse)
                {
                    TempData["Error"] =
                        "True/False questions can only have True or False options.";

                    return RedirectToAction(
                        nameof(Edit),
                        new { id = model.QuestionId });
                }


                if (model.IsCorrect &&
                    question.QuestionOptions.Any(o =>
                        o.IsCorrect))
                {
                    TempData["Error"] =
                        "A True/False question can have only one correct answer.";

                    return RedirectToAction(
                        nameof(Edit),
                        new { id = model.QuestionId });
                }
            }
            if (question.QuestionType ==
                QuestionType.MultipleChoice)
            {
                if (model.IsCorrect &&
                    question.QuestionOptions.Any(o =>
                        o.IsCorrect))
                {
                    TempData["Error"] =
                        "This question can have only one correct answer.";

                    return RedirectToAction(
                        nameof(Edit),
                        new { id = model.QuestionId });
                }
            }
            var option = new QuestionOption
            {
                OptionText =
                    optionText,

                IsCorrect =
                    model.IsCorrect,

                QuestionId =
                    model.QuestionId
            };


            await _questionOptionRepository.AddAsync(option);


            if (await _questionOptionRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Question option added successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Something went wrong.";
            }


            return RedirectToAction(
                nameof(Edit),
                new { id = model.QuestionId });
        }
        [HttpGet]
        public async Task<IActionResult> EditOption(int id)
        {
            var option =
                await _questionOptionRepository.GetOneAsync(
                    o => o.Id == id);


            if (option == null)
                return NotFound();


            var model = new QuestionOptionVM
            {
                Id =
                    option.Id,

                OptionText =
                    option.OptionText,

                IsCorrect =
                    option.IsCorrect,

                QuestionId =
                    option.QuestionId
            };


            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOption(
            QuestionOptionVM model)
        {
            if (!ModelState.IsValid)
                return View(model);


            if (string.IsNullOrWhiteSpace(model.OptionText))
            {
                ModelState.AddModelError(
                    nameof(model.OptionText),
                    "Option text is required.");

                return View(model);
            }


            var option =
                await _questionOptionRepository.GetOneAsync(
                    o => o.Id == model.Id);


            if (option == null)
                return NotFound();


            if (option.QuestionId != model.QuestionId)
                return BadRequest();


            var question =
                await _questionRepository.GetOneAsync(
                    q => q.Id == option.QuestionId,
                    includes:
                    [
                        q => q.QuestionOptions
                    ]);


            if (question == null)
                return NotFound();


            string optionText =
                model.OptionText.Trim();

            bool duplicateOption =
                question.QuestionOptions.Any(o =>
                    o.Id != option.Id &&
                    o.OptionText.Equals(
                        optionText,
                        StringComparison.OrdinalIgnoreCase));


            if (duplicateOption)
            {
                ModelState.AddModelError(
                    nameof(model.OptionText),
                    "This option already exists.");

                return View(model);
            }
            if (question.QuestionType ==
                QuestionType.TrueFalse)
            {
                bool validTrueFalse =
                    optionText.Equals(
                        "True",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    optionText.Equals(
                        "False",
                        StringComparison.OrdinalIgnoreCase);


                if (!validTrueFalse)
                {
                    ModelState.AddModelError(
                        nameof(model.OptionText),
                        "True/False questions can only have True or False options.");

                    return View(model);
                }


                if (model.IsCorrect &&
                    question.QuestionOptions.Any(o =>
                        o.Id != option.Id &&
                        o.IsCorrect))
                {
                    ModelState.AddModelError(
                        nameof(model.IsCorrect),
                        "A True/False question can have only one correct answer.");

                    return View(model);
                }
            }
            if (question.QuestionType ==
                QuestionType.MultipleChoice)
            {
                if (model.IsCorrect &&
                    question.QuestionOptions.Any(o =>
                        o.Id != option.Id &&
                        o.IsCorrect))
                {
                    ModelState.AddModelError(
                        nameof(model.IsCorrect),
                        "This question can have only one correct answer.");

                    return View(model);
                }
            }
            option.OptionText =
                optionText;

            option.IsCorrect =
                model.IsCorrect;


            _questionOptionRepository.Update(option);


            if (await _questionOptionRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Question option updated successfully.";

                return RedirectToAction(
                    nameof(Edit),
                    new { id = option.QuestionId });
            }


            TempData["Error"] =
                "Something went wrong.";


            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOption(int id)
        {
            var option =
                await _questionOptionRepository.GetOneAsync(
                    o => o.Id == id);


            if (option == null)
                return NotFound();


            int questionId =
                option.QuestionId;


            _questionOptionRepository.Delete(option);


            if (await _questionOptionRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Question option deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Something went wrong.";
            }


            return RedirectToAction(
                nameof(Edit),
                new { id = questionId });
        }
        private async Task LoadQuestionData(
            QuestionVM? model = null)
        {
            var quizzes =
                await _quizRepository.GetAsync(
                    tracked: false);


            ViewBag.Quizzes =
                new SelectList(
                    quizzes.OrderBy(q => q.Title),
                    "Id",
                    "Title",
                    model?.QuizId);
        }
        private async Task LoadQuestionOptions(
            QuestionVM model)
        {
            var question =
                await _questionRepository.GetOneAsync(
                    q => q.Id == model.Id,
                    includes:
                    [
                        q => q.QuestionOptions
                    ]);


            if (question == null)
            {
                model.QuestionOptions =
                    new List<QuestionOptionVM>();

                return;
            }


            model.QuestionOptions =
                question.QuestionOptions
                    .Select(o => new QuestionOptionVM
                    {
                        Id =
                            o.Id,

                        OptionText =
                            o.OptionText,

                        IsCorrect =
                            o.IsCorrect,

                        QuestionId =
                            o.QuestionId
                    })
                    .ToList();
        }
        private async Task<IActionResult> ReturnCreateView(
            QuestionVM model)
        {
            model.QuestionOptions ??=
                new List<QuestionOptionVM>();


            await LoadQuestionData(model);


            return View(model);
        }
    }
}
