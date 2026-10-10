using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    [Authorize(Roles = $"{RoleNames.SUPER_ADMIN}")]

    public class QuestionsController : Controller
    {
        private readonly IRepository<Question> _questionRepository;
        private readonly IRepository<Quiz> _quizRepository;
        private readonly IRepository<QuestionOption> _questionOptionRepository;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private const int PageSize = 6;

        public QuestionsController(
            IRepository<Question> questionRepository,
            IRepository<Quiz> quizRepository,
            IRepository<QuestionOption> questionOptionRepository,
            IStringLocalizer<SharedResource> localizer)
        {
            _questionRepository = questionRepository;
            _quizRepository = quizRepository;
            _questionOptionRepository = questionOptionRepository;
            _localizer = localizer;
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
                    _localizer["SelectedQuizDoesNotExist"]);

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
                    _localizer["PleaseAddAtLeastOneOption"]);

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
                    _localizer["QuestionOptionsCannotBeDuplicated"]);

                return await ReturnCreateView(model);
            }
            int correctAnswers =
                options.Count(o => o.IsCorrect);


            if (correctAnswers == 0)
            {
                ModelState.AddModelError(
                    nameof(model.QuestionOptions),
                    _localizer["PleaseSelectOneCorrectAnswer"]);

                return await ReturnCreateView(model);
            }
            if (model.QuestionType ==
                QuestionType.MultipleChoice)
            {
                if (options.Count < 2)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionOptions),
                        _localizer[
                            "MultipleChoiceNeedsAtLeastTwoOptions"]);

                    return await ReturnCreateView(model);
                }

                if (correctAnswers > 1)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionOptions),
                        _localizer[
                            "MultipleChoiceCanHaveOnlyOneCorrectAnswer"]);

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
                        _localizer["TrueFalseMustHaveExactlyTwoOptions"]);

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
                        _localizer[
                            "TrueFalseOnlyTrueAndFalseOptions"]);

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
                    _localizer["TrueFalseMustContainBothTrueAndFalse"]);

                    return await ReturnCreateView(model);
                }
                if (correctAnswers != 1)
                {
                    ModelState.AddModelError(
                    nameof(model.QuestionOptions),
                    _localizer["TrueFalseExactlyOneCorrectAnswer"]);

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
                TempData["error"] =
                    _localizer["SomethingWentWrongCreatingQuestion"]
                        .ToString();

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
                TempData["error"] =
                    _localizer["QuestionCreatedOptionsFailed"].ToString();

                return RedirectToAction(
                    nameof(Edit),
                    new { id = question.Id });
            }


            TempData["success"] =
                _localizer["QuestionAndOptionsCreatedSuccessfully"]
                    .ToString();


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
            model.QuestionOptions ??= new List<QuestionOptionVM>();

            if (!ModelState.IsValid)
            {
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
                    _localizer["SelectedQuizDoesNotExist"]);

                await LoadQuestionData(model);
                return View(model);
            }

            var submittedOptions = model.QuestionOptions
                .Where(o => !string.IsNullOrWhiteSpace(o.OptionText))
                .ToList();

            if (!submittedOptions.Any())
            {
                ModelState.AddModelError(
                    nameof(model.QuestionOptions),
                    _localizer["PleaseAddAtLeastOneOption"]);

                await LoadQuestionData(model);
                return View(model);
            }

            foreach (var option in submittedOptions)
            {
                option.OptionText = option.OptionText.Trim();
            }

            bool duplicateOptions = submittedOptions
                .GroupBy(
                    o => o.OptionText,
                    StringComparer.OrdinalIgnoreCase)
                .Any(g => g.Count() > 1);

            if (duplicateOptions)
            {
                ModelState.AddModelError(
                    nameof(model.QuestionOptions),
                    _localizer["QuestionOptionsCannotBeDuplicated"]);

                await LoadQuestionData(model);
                return View(model);
            }

            int correctAnswers =
                submittedOptions.Count(o => o.IsCorrect);

            if (correctAnswers == 0)
            {
                ModelState.AddModelError(
                    nameof(model.QuestionOptions),
                    _localizer["PleaseSelectOneCorrectAnswer"]);

                await LoadQuestionData(model);
                return View(model);
            }
            if (model.QuestionType == QuestionType.MultipleChoice)
            {
                if (submittedOptions.Count < 2)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionOptions),
                        _localizer[
                            "MultipleChoiceNeedsAtLeastTwoOptions"]);

                    await LoadQuestionData(model);
                    return View(model);
                }

                if (correctAnswers != 1)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionOptions),
                        _localizer[
                            "MultipleChoiceMustHaveExactlyOneCorrectAnswer"]);

                    await LoadQuestionData(model);
                    return View(model);
                }
            }

            if (model.QuestionType == QuestionType.TrueFalse)
            {
                if (submittedOptions.Count != 2)
                {
                    ModelState.AddModelError(
                        nameof(model.QuestionOptions),
                        _localizer["TrueFalseMustHaveExactlyTwoOptions"]);

                    await LoadQuestionData(model);
                    return View(model);
                }

                bool validTrueFalse = submittedOptions.All(o =>
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
                        _localizer[
                            "TrueFalseOnlyTrueAndFalseOptions"]);

                    await LoadQuestionData(model);
                    return View(model);
                }

                bool hasTrue = submittedOptions.Any(o =>
                    o.OptionText.Equals(
                        "True",
                        StringComparison.OrdinalIgnoreCase));

                bool hasFalse = submittedOptions.Any(o =>
                    o.OptionText.Equals(
                        "False",
                        StringComparison.OrdinalIgnoreCase));

                if (!hasTrue || !hasFalse)
                {
                    ModelState.AddModelError(
                    nameof(model.QuestionOptions),
                    _localizer["TrueFalseMustContainBothTrueAndFalse"]);

                    await LoadQuestionData(model);
                    return View(model);
                }

                if (correctAnswers != 1)
                {
                    ModelState.AddModelError(
                    nameof(model.QuestionOptions),
                    _localizer["TrueFalseExactlyOneCorrectAnswer"]);

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

            var oldOptions =
                question.QuestionOptions?.ToList()
                ?? new List<QuestionOption>();

            foreach (var oldOption in oldOptions)
            {
                _questionOptionRepository.Delete(oldOption);
            }

            foreach (var submittedOption in submittedOptions)
            {
                var newOption = new QuestionOption
                {
                    OptionText =
                        submittedOption.OptionText.Trim(),

                    IsCorrect =
                        submittedOption.IsCorrect,

                    QuestionId =
                        question.Id
                };

                await _questionOptionRepository.AddAsync(newOption);
            }

            int result =
                await _questionOptionRepository.CommitAsync();

            if (result > 0)
            {
                TempData["success"] =
                    _localizer["QuestionAndOptionsUpdatedSuccessfully"]
                        .ToString();

                return RedirectToAction(nameof(Index));
            }

            TempData["error"] =
                _localizer["SomethingWentWrongUpdatingQuestion"]
                    .ToString();

            await LoadQuestionData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _questionRepository.GetOneAsync(
                q => q.Id == id);

            if (question == null)
                return NotFound();

            _questionRepository.Delete(question);

            int result = await _questionRepository.CommitAsync();

            if (result > 0)
            {
                TempData["success"] =
                    _localizer["QuestionAndOptionsDeletedSuccessfully"]
                        .ToString();
            }
            else
            {
                TempData["error"] =
                    _localizer["SomethingWentWrongDeletingQuestion"]
                        .ToString();
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
                    _localizer["PleaseEnterValidOptionData"].ToString();

                return RedirectToAction(
                    nameof(Edit),
                    new { id = model.QuestionId });
            }


            if (string.IsNullOrWhiteSpace(model.OptionText))
            {
                TempData["Error"] =
                    _localizer["OptionTextRequired"].ToString();

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
                    _localizer["OptionAlreadyExists"].ToString();

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
                        _localizer["TrueFalseOnlyTwoOptions"].ToString();

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
                        _localizer["TrueFalseOnlyTrueOrFalseOptions"]
                            .ToString();

                    return RedirectToAction(
                        nameof(Edit),
                        new { id = model.QuestionId });
                }


                if (model.IsCorrect &&
                    question.QuestionOptions.Any(o =>
                        o.IsCorrect))
                {
                    TempData["Error"] =
                        _localizer["TrueFalseQuestionOnlyOneCorrectAnswer"]
                            .ToString();

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
                        _localizer["QuestionOnlyOneCorrectAnswer"]
                            .ToString();

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
                    _localizer["QuestionOptionAddedSuccessfully"]
                        .ToString();
            }
            else
            {
                TempData["Error"] =
                    _localizer["SomethingWentWrong"].ToString();
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
                    _localizer["OptionTextRequired"]);

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
                    _localizer["OptionAlreadyExists"]);

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
                        _localizer[
                            "TrueFalseOnlyTrueOrFalseOptions"]);

                    return View(model);
                }


                if (model.IsCorrect &&
                    question.QuestionOptions.Any(o =>
                        o.Id != option.Id &&
                        o.IsCorrect))
                {
                    ModelState.AddModelError(
                        nameof(model.IsCorrect),
                        _localizer[
                            "TrueFalseQuestionOnlyOneCorrectAnswer"]);

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
                        _localizer[
                            "QuestionOnlyOneCorrectAnswer"]);

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
                    _localizer["QuestionOptionUpdatedSuccessfully"]
                        .ToString();

                return RedirectToAction(
                    nameof(Edit),
                    new { id = option.QuestionId });
            }


            TempData["Error"] =
                _localizer["SomethingWentWrong"].ToString();


            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOption(int id)
        {
            var option = await _questionOptionRepository.GetOneAsync(
                o => o.Id == id);

            if (option == null)
                return NotFound();

            int questionId = option.QuestionId;

            var question = await _questionRepository.GetOneAsync(
                q => q.Id == questionId,
                includes:
                [
                    q => q.QuestionOptions
                ]);

            if (question == null)
                return NotFound();

            if (question.QuestionOptions.Count <= 1)
            {
                TempData["error"] =
                    _localizer["QuestionMustHaveAtLeastOneOption"]
                        .ToString();

                return RedirectToAction(
                    nameof(Edit),
                    new { id = questionId });
            }

            _questionOptionRepository.Delete(option);

            int result = await _questionOptionRepository.CommitAsync();

            if (result > 0)
            {
                TempData["success"] =
                    _localizer["QuestionOptionDeletedSuccessfully"]
                        .ToString();
            }
            else
            {
                TempData["error"] =
                    _localizer["SomethingWentWrongDeletingOption"]
                        .ToString();
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
