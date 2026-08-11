using Microsoft.AspNetCore.Mvc;

namespace Training_Platform.Areas.Admin.Controllers
{
    
    [Area(SD.Admin_Area)]
    public class CertificatesController : Controller
    {
        private readonly IRepository<Certificate> _certificateRepository;

        public CertificatesController(IRepository<Certificate> certificateRepository)
        {
            _certificateRepository = certificateRepository;
        }
        public async Task<IActionResult> Index(
        int page = 1,
        string? query = null,
        CancellationToken cancellationToken = default)
        {
            var certificates = await _certificateRepository.GetAsync(
                includes:
                [
                    c => c.User,
            c => c.Course
                ],
                tracked: false,
                cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim().ToLower();

                certificates = certificates.Where(c =>
                    c.CertificateNumber.ToLower().Contains(query) ||
                    c.User.UserName!.ToLower().Contains(query) ||
                    c.Course.Title.ToLower().Contains(query));
            }

            int totalPages = (int)Math.Ceiling(certificates.Count() / 6.0);

            certificates = certificates
                .Skip((page - 1) * 6)
                .Take(6);

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

            return View(certificate);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
        {
            var certificate = await _certificateRepository.GetOneAsync(
                c => c.Id == id,
                cancellationToken: cancellationToken);

            if (certificate is null)
                return NotFound();

            _certificateRepository.Delete(certificate);

            await _certificateRepository.CommitAsync(cancellationToken);

            TempData["success_notification"] = "Certificate deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
