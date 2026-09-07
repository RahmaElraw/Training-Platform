using System.Security.Claims;

[Area(SD.Trainee_Area)]
public class CertificatesController : Controller
{
    private readonly IRepository<Certificate> _certificateRepository;

    public CertificatesController(
        IRepository<Certificate> certificateRepository)
    {
        _certificateRepository = certificateRepository;
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