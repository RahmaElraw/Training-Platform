using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Training_Platform.DTO;
using Training_Platform.Service.IService;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    [Authorize(Roles = $"{RoleNames.SUPER_ADMIN}")]

    public class QuizController : Controller
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [HttpGet]
        public async Task<IActionResult> Take(
            int id,
            CancellationToken cancellationToken)
        {
            var quiz = await _quizService.GetQuizForTakingAsync(
                id,
                cancellationToken);

            if (quiz == null)
                return NotFound();

            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            SubmitQuizDto dto,
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int userId))
            {
                return Unauthorized();
            }

            var resultId = await _quizService.SubmitQuizAsync(
                dto.QuizId,
                userId,
                dto,
                cancellationToken);

            if (resultId == null)
                return NotFound();

            return RedirectToAction(
                nameof(Result),
                new { id = resultId.Value });
        }

        [HttpGet]
        public async Task<IActionResult> Result(
            int id,
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int userId))
            {
                return Unauthorized();
            }

            var result = await _quizService.GetResultAsync(
                id,
                userId,
                cancellationToken);

            if (result == null)
                return NotFound();

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Results(
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int userId))
            {
                return Unauthorized();
            }

            var results = await _quizService.GetMyResultsAsync(
                userId,
                cancellationToken);

            return View(results);
        }
    }
}
