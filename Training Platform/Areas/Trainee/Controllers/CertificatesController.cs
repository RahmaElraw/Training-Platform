using Microsoft.Extensions.Localization;
using System.Security.Claims;
using Training_Platform;

[Area(SD.Trainee_Area)]
[Authorize(Roles = RoleNames.TRAINEE)]

public class CertificatesController : Controller
{
    private readonly IRepository<Certificate> _certificateRepository;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CertificatesController(
        IRepository<Certificate> certificateRepository,
        IStringLocalizer<SharedResource> localizer)
    {
        _certificateRepository = certificateRepository;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken = default)
    {
        var userId = int.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var certificates = await _certificateRepository.GetAsync(
            c => c.UserId == userId,
            includes:
            [
                c => c.Course
            ],
            tracked: false,
            cancellationToken: cancellationToken
        );

        return View(certificates);
    }
}