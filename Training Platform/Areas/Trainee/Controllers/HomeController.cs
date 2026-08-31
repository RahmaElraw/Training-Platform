using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Training_Platform.ViewModels.Trainee;

namespace Training_Platform.Areas.Trainee.Controllers
{
    [Area(SD.Trainee_Area)]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IRepository<Enrollment> _enrollmentRepository;
        private readonly IRepository<Certificate> _certificateRepository;
        private readonly IRepository<UserProgress> _userProgressRepository;

        public HomeController(
            IRepository<Enrollment> enrollmentRepository,
            IRepository<Certificate> certificateRepository,
            IRepository<UserProgress> userProgressRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _certificateRepository = certificateRepository;
            _userProgressRepository = userProgressRepository;
        }

        public async Task<IActionResult> Index(
            CancellationToken cancellationToken = default)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            // 1. Fetch enrollments with course lessons
            var enrollments = await _enrollmentRepository.GetAsync(
                e => e.UserId == userId,
                includes:
                [
                    e => e.Course,
                    e => e.Course.Lessons
                ],
                tracked: false,
                cancellationToken: cancellationToken
            );

            // 2. Fetch certificates
            var certificates = await _certificateRepository.GetAsync(
                c => c.UserId == userId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            // 3. Fetch completed lesson records for this user
            var userProgresses = await _userProgressRepository.GetAsync(
                up => up.UserId == userId && up.IsCompleted,
                tracked: false,
                cancellationToken: cancellationToken
            );

            var completedLessonIds = userProgresses
                .Select(up => up.LessonId)
                .ToHashSet();

            var enrollmentList = enrollments.ToList();

            // 4. Calculate progress per course
            var courseProgresses = enrollmentList.Select(e =>
            {
                var totalLessons = e.Course?.Lessons?.Count ?? 0;
                var completedLessons = e.Course?.Lessons?
                    .Count(l => completedLessonIds.Contains(l.Id)) ?? 0;

                var percentage = totalLessons > 0
                    ? (int)Math.Round((double)completedLessons / totalLessons * 100)
                    : 0;

                return new CourseProgressItemVM
                {
                    CourseTitle = e.Course?.Title ?? "N/A",
                    CompletedLessons = completedLessons,
                    TotalLessons = totalLessons,
                    ProgressPercentage = percentage
                };
            }).ToList();

            var vm = new TraineeDashboardVM
            {
                TotalEnrollments = enrollmentList.Count,
                CompletedCourses = enrollmentList.Count(e => e.IsCompleted),
                InProgressCourses = enrollmentList.Count(e => !e.IsCompleted),
                TotalCertificates = certificates.Count(),
                CourseProgresses = courseProgresses,
                RecentEnrollments = enrollmentList
                    .OrderByDescending(e => e.EnrollmentDate)
                    .Take(5)
                    .ToList()
            };

            return View(vm);
        }
    }
}