using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

[Area(SD.Trainer_Area)]
public class QuizzesController : Controller
{
    private readonly IRepository<Quiz> _quizRepository;
    private readonly IQuizRepository _quizDetailsRepository;
    private readonly IRepository<Course> _courseRepository;

    public QuizzesController(
    IRepository<Quiz> quizRepository,
    IQuizRepository quizDetailsRepository,
    IRepository<Course> courseRepository)
    {
        _quizRepository = quizRepository;
        _quizDetailsRepository = quizDetailsRepository;
        _courseRepository = courseRepository;
    }

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
                q => q.Course,
                q => q.Questions
            ],
            tracked: false,
            cancellationToken: cancellationToken);

        quizzes = quizzes
            .OrderBy(q => q.Course.Title)
            .ThenBy(q => q.Title);

        return View(quizzes);
    }

    [HttpGet]
    public async Task<IActionResult> Create(
    CancellationToken cancellationToken)
    {
        int trainerId = GetCurrentTrainerId();

        if (trainerId <= 0)
            return Unauthorized();

        var courses = await GetTrainerCourses(
            trainerId,
            cancellationToken);

        if (!courses.Any())
        {
            TempData["Warning"] =
                "You cannot create a quiz because you do not have any courses yet.";

            return RedirectToAction(nameof(Index));
        }

        await LoadCourses(courses);

        return View(new QuizVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    QuizVM model,
    CancellationToken cancellationToken)
    {
        int trainerId = GetCurrentTrainerId();

        if (trainerId <= 0)
            return Unauthorized();


        // Make sure the selected Course belongs
        // to the current Trainer
        var course = await _courseRepository.GetOneAsync(
            c => c.Id == model.CourseId &&
                 c.TrainerId == trainerId,
            tracked: false,
            cancellationToken: cancellationToken);


        if (course == null)
        {
            ModelState.AddModelError(
                nameof(model.CourseId),
                "The selected course does not belong to you.");

            await LoadCourses(
                await GetTrainerCourses(
                    trainerId,
                    cancellationToken));

            return View(model);
        }


        // Validate ViewModel
        if (!ModelState.IsValid)
        {
            await LoadCourses(
                await GetTrainerCourses(
                    trainerId,
                    cancellationToken));

            return View(model);
        }


        // Check duplicate Quiz title
        // inside the same Course
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
                "You already have a quiz with this title in this course.");

            await LoadCourses(
                await GetTrainerCourses(
                    trainerId,
                    cancellationToken));

            return View(model);
        }


        // Create Quiz
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


        int result = await _quizRepository.CommitAsync(
            cancellationToken);


        if (result > 0)
        {
            TempData["Success"] =
                "Quiz created successfully.";

            return RedirectToAction(nameof(Index));
        }


        TempData["Error"] =
            "Something went wrong while creating the quiz.";


        await LoadCourses(
            await GetTrainerCourses(
                trainerId,
                cancellationToken));

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

        var quiz = await _quizDetailsRepository.GetQuizForTrainerDetailsAsync(
            id,
            trainerId,
            cancellationToken);

        if (quiz == null)
            return NotFound();

        return View(quiz);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
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
            tracked: false,
            cancellationToken: cancellationToken);


        if (quiz == null)
            return NotFound();


        if (quiz.Course == null ||
            quiz.Course.TrainerId != trainerId)
        {
            return NotFound();
        }


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
    public async Task<IActionResult> Edit(
        QuizVM model,
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


        ViewBag.Course = course;

        var quiz = await _quizRepository.GetOneAsync(
            q => q.Id == model.Id &&
                 q.CourseId == model.CourseId,
            cancellationToken: cancellationToken);


        if (quiz == null)
            return NotFound();


        if (!ModelState.IsValid)
            return View(model);

        string normalizedTitle = model.Title.Trim().ToLower();

        var existingQuiz = await _quizRepository.GetOneAsync(
            q => q.CourseId == model.CourseId &&
                 q.Title.ToLower() == normalizedTitle &&
                 q.Id != model.Id,
            tracked: false,
            cancellationToken: cancellationToken);


        if (existingQuiz != null)
        {
            ModelState.AddModelError(
                nameof(model.Title),
                "You already have a quiz with this title in this course.");

            return View(model);
        }


        quiz.Title = model.Title.Trim();
        quiz.PassingScore = model.PassingScore;
        quiz.TimeLimit = model.TimeLimit;


        _quizRepository.Update(quiz);


        int result = await _quizRepository.CommitAsync(
            cancellationToken);


        if (result > 0)
        {
            TempData["Success"] =
                "Quiz updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = quiz.Id
                });
        }


        TempData["Error"] =
            "Something went wrong while updating the quiz.";

        return View(model);
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


        var quiz = await _quizRepository.GetOneAsync(
            q => q.Id == id,
            includes:
            [
                q => q.Course,
                q => q.Questions,
                q => q.QuizResults
            ],
            cancellationToken: cancellationToken);


        if (quiz == null)
            return NotFound();


        if (quiz.Course == null ||
            quiz.Course.TrainerId != trainerId)
        {
            return NotFound();
        }


        int questionsCount =
            quiz.Questions?.Count ?? 0;

        int resultsCount =
            quiz.QuizResults?.Count ?? 0;


        int courseId = quiz.CourseId;


        _quizRepository.Delete(quiz);


        int result = await _quizRepository.CommitAsync(
            cancellationToken);


        if (result > 0)
        {
            TempData["Success"] =
                questionsCount > 0 || resultsCount > 0
                    ? $"Quiz deleted successfully. " +
                      $"{questionsCount} question(s) and " +
                      $"{resultsCount} result(s) were also deleted."
                    : "Quiz deleted successfully.";
        }
        else
        {
            TempData["Error"] =
                "Something went wrong while deleting the quiz.";
        }

        return RedirectToAction(
            "Details",
            "Courses",
            new
            {
                area = SD.Trainer_Area,
                id = courseId
            });
    }

    private int GetCurrentTrainerId()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdValue, out int trainerId)
            ? trainerId
            : 0;
    }

    private async Task<IEnumerable<Course>> GetTrainerCourses(
    int trainerId,
    CancellationToken cancellationToken)
    {
        var courses = await _courseRepository.GetAsync(
            c => c.TrainerId == trainerId,
            tracked: false,
            cancellationToken: cancellationToken);

        return courses
            .OrderBy(c => c.Title)
            .ToList();
    }

    private async Task LoadCourses(
    IEnumerable<Course> courses)
    {
        ViewBag.Courses = courses
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Title
            })
            .ToList();
    }
}