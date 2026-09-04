using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    [Authorize(Roles = $"{RoleNames.SUPER_ADMIN}")]

    public class CertificatesController : Controller
    {
        private readonly IRepository<Certificate> _certificateRepository;
        private readonly IRepository<Enrollment> _enrollmentRepository;

        public CertificatesController(
            IRepository<Certificate> certificateRepository,
            IRepository<Enrollment> enrollmentRepository)
        {
            _certificateRepository = certificateRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            string? query = null,
            CancellationToken cancellationToken = default)
        {
            const int pageSize = 6;

            // Get all completed course enrollments
            var completedEnrollments = await _enrollmentRepository.GetAsync(
                e => e.IsCompleted,
                includes:
                [
                    e => e.User,
                    e => e.Course
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            if (!completedEnrollments.Any())
            {
                return View(new CertificateWithRelatedVM
                {
                    Certificates = [],
                    CurrentPage = 1,
                    TotalPages = 0,
                    Query = query
                });
            }

            // Fetch existing certificates
            var existingCertificates = await _certificateRepository.GetAsync(
                includes:
                [
                    c => c.User,
                    c => c.Course
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            var existingKeys = existingCertificates
                .Select(c => (c.UserId, c.CourseId))
                .ToHashSet();

            // Auto-generate Certificate records for any completed enrollment that lacks one
            var missingEnrollments = completedEnrollments
                .Where(e => !existingKeys.Contains((e.UserId, e.CourseId)))
                .ToList();

            if (missingEnrollments.Any())
            {
                foreach (var enrollment in missingEnrollments)
                {
                    var newCertificate = new Certificate
                    {
                        UserId = enrollment.UserId,
                        CourseId = enrollment.CourseId,
                        CertificateNumber = $"CRT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                        IssueDate = DateTime.UtcNow,
                        CertificateUrl = null
                    };

                    await _certificateRepository.AddAsync(newCertificate, cancellationToken);
                }

                await _certificateRepository.CommitAsync(cancellationToken);

                // Refresh certificate list
                existingCertificates = await _certificateRepository.GetAsync(
                    includes:
                    [
                        c => c.User,
                        c => c.Course
                    ],
                    tracked: false,
                    cancellationToken: cancellationToken);
            }

            //Filter certificates matching completed enrollments
            var completedKeys = completedEnrollments
                .Select(e => (e.UserId, e.CourseId))
                .ToHashSet();

            var certificates = existingCertificates
                .Where(c => completedKeys.Contains((c.UserId, c.CourseId)))
                .AsEnumerable();

            // Search
            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim().ToLower();

                certificates = certificates.Where(c =>
                    c.CertificateNumber.ToLower().Contains(query) ||
                    (c.User?.UserName != null && c.User.UserName.ToLower().Contains(query)) ||
                    (c.User?.Email != null && c.User.Email.ToLower().Contains(query)) ||
                    (c.Course?.Title != null && c.Course.Title.ToLower().Contains(query)));
            }

            // Pagination
            int totalPages = (int)Math.Ceiling(
                certificates.Count() / (double)pageSize);

            if (page < 1)
                page = 1;

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            certificates = certificates
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return View(new CertificateWithRelatedVM
            {
                Certificates = certificates,
                CurrentPage = page,
                TotalPages = totalPages,
                Query = query
            });
        }

        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken = default)
        {
            var certificate = await _certificateRepository.GetOneAsync(
                c => c.Id == id,
                includes:
                [
                    c => c.User,
                    c => c.Course
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            if (certificate is null)
                return NotFound();

            var model = new CertificateVM
            {
                Id = certificate.Id,
                CertificateNumber = certificate.CertificateNumber,
                IssueDate = certificate.IssueDate,
                UserName = certificate.User.UserName ?? string.Empty,
                UserEmail = certificate.User.Email ?? string.Empty,
                CourseTitle = certificate.Course.Title,
                CertificateUrl = certificate.CertificateUrl
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id,
            CancellationToken cancellationToken = default)
        {
            var certificate = await _certificateRepository.GetOneAsync(
                c => c.Id == id,
                tracked: false,
                cancellationToken: cancellationToken);

            if (certificate is null)
                return NotFound();

            var model = new CertificateVM
            {
                Id = certificate.Id,
                CertificateNumber = certificate.CertificateNumber,
                IssueDate = certificate.IssueDate,
                CertificateUrl = certificate.CertificateUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            CertificateVM model,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View(model);

            var certificate = await _certificateRepository.GetOneAsync(
                c => c.Id == model.Id,
                tracked: true,
                cancellationToken: cancellationToken);

            if (certificate is null)
                return NotFound();

            // Update Google Drive URL
            certificate.CertificateUrl = model.CertificateUrl;

            var result = await _certificateRepository.CommitAsync(cancellationToken);

            if (result <= 0)
            {
                TempData["error_notification"] =
                    "Something went wrong while updating the certificate link.";

                return View(model);
            }

            TempData["success_notification"] =
                "Certificate link updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = certificate.Id });
        }
    }
}