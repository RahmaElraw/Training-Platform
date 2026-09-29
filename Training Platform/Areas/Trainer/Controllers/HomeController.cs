using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Training_Platform.Areas.Trainer;

namespace Training_Platform.Areas.Trainer.Controllers
{
    [Area(SD.Trainer_Area)]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            var userIdString =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int trainerId))
                return Unauthorized();

            var trainerCourses = _context.Courses
                .Where(c => c.TrainerId == trainerId);

            var vm = new TrainerHomeVM
            {
                MyCourses = await trainerCourses
                    .CountAsync(cancellationToken),

                PublishedCourses = await trainerCourses
                    .CountAsync(
                        c => c.IsPublished,
                        cancellationToken),

                MyQuizzes = await _context.Quizzes
                    .Where(q => q.Course.TrainerId == trainerId)
                    .CountAsync(cancellationToken),

                TotalTrainees = await _context.Enrollments
                    .Where(e => e.Course.TrainerId == trainerId)
                    .Select(e => e.UserId)
                    .Distinct()
                    .CountAsync(cancellationToken),

                CompletedEnrollments = await _context.Enrollments
                    .Where(e =>
                        e.Course.TrainerId == trainerId &&
                        e.IsCompleted)
                    .CountAsync(cancellationToken),

                InProgressEnrollments = await _context.Enrollments
                    .Where(e =>
                        e.Course.TrainerId == trainerId &&
                        !e.IsCompleted)
                    .CountAsync(cancellationToken),

                CertificatesIssued = await _context.Certificates
                    .Where(c => c.Course.TrainerId == trainerId)
                    .CountAsync(cancellationToken)
            };

            vm.CourseStatistics = await trainerCourses

                .OrderByDescending(c => c.Enrollments.Count)

                .Take(6)

                .Select(c => new CourseStatisticVM
                {
                    CourseTitle = c.Title,

                    TraineesCount =
                        c.Enrollments.Count,

                    CompletedCount =
                        c.Enrollments.Count(
                            e => e.IsCompleted),

                    InProgressCount =
                        c.Enrollments.Count(
                            e => !e.IsCompleted)
                })

                .ToListAsync(cancellationToken);

            vm.RecentCourses = await trainerCourses

                .OrderByDescending(c => c.CreatedAt)

                .Take(5)

                .Select(c => new RecentCourseVM
                {
                    Id = c.Id,

                    Title = c.Title,

                    IsPublished =
                        c.IsPublished,

                    CreatedAt =
                        c.CreatedAt,

                    TraineesCount =
                        c.Enrollments.Count
                })

                .ToListAsync(cancellationToken);


            return View(vm);
        }
    }
}