using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Training_Platform.Repositories.IRepositories;

namespace Training_Platform.Areas.Trainer.Controllers
{
    [Area(SD.Trainer_Area)]
    public class ProgressController : Controller
    {
        private readonly IProgressRepository _progressRepository;

        public ProgressController(
            IProgressRepository progressRepository)
        {
            _progressRepository = progressRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var progress =
                await _progressRepository.GetTrainerProgressAsync(
                    trainerId,
                    cancellationToken);

            return View(progress);
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