using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Training_Platform.Models;
using Training_Platform.Repositories.IRepositories;
using Training_Platform.Services;

namespace Training_Platform.Areas.Trainer.Controllers
{
    [Area(SD.Trainer_Area)]
    public class ProgressController : Controller
    {
        private readonly IProgressRepository _progressRepository;
        private readonly IRepository<Enrollment> _enrollmentRepository;
        private readonly IRepository<Certificate> _certificateRepository;
        private readonly ICertificateService _certificateService;

        public ProgressController(
            IProgressRepository progressRepository,
            IRepository<Enrollment> enrollmentRepository,
            IRepository<Certificate> certificateRepository,
            ICertificateService certificateService)
        {
            _progressRepository = progressRepository;
            _enrollmentRepository = enrollmentRepository;
            _certificateRepository = certificateRepository;
            _certificateService = certificateService;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateCertificate(
            int enrollmentId,
            CancellationToken cancellationToken)
        {
            int trainerId = GetCurrentTrainerId();

            if (trainerId <= 0)
                return Unauthorized();

            var enrollment = await _enrollmentRepository.GetOneAsync(
                e => e.Id == enrollmentId &&
                     e.Course.TrainerId == trainerId,

                includes: new Expression<Func<Enrollment, object>>[]
                {
                    e => e.Course,
                    e => e.User
                },

                tracked: true,

                cancellationToken: cancellationToken
            );

            if (enrollment == null)
            {
                TempData["Error"] =
                    "Enrollment not found.";

                return RedirectToAction(nameof(Index));
            }

            if (!enrollment.IsCompleted)
            {
                TempData["Error"] =
                    "The trainee has not completed this course yet.";

                return RedirectToAction(nameof(Index));
            }

            var existingCertificate =
                await _certificateRepository.GetOneAsync(
                    c => c.UserId == enrollment.UserId &&
                         c.CourseId == enrollment.CourseId,

                    tracked: false,

                    cancellationToken: cancellationToken
                );


            if (existingCertificate != null)
            {
                TempData["Error"] =
                    "A certificate has already been generated for this trainee.";

                return RedirectToAction(nameof(Index));
            }

            var certificate = new Certificate
            {
                CertificateNumber =
                    $"CERT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",

                IssueDate = DateTime.UtcNow,

                UserId = enrollment.UserId,

                CourseId = enrollment.CourseId,

                CertificateUrl = null
            };

            var certificateUrl =
                await _certificateService.GenerateCertificateAsync(
                    certificate,
                    enrollment.User,
                    enrollment.Course,
                    cancellationToken);

            certificate.CertificateUrl = certificateUrl;

            await _certificateRepository.AddAsync(
                certificate,
                cancellationToken);

            await _certificateRepository.CommitAsync(
                cancellationToken);


            TempData["Success"] =
                "Certificate generated successfully.";

            return RedirectToAction(nameof(Index));
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