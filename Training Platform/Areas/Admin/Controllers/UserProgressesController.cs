using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    [Authorize(Roles = $"{RoleNames.SUPER_ADMIN}")]

    public class UserProgressesController : Controller
    {
        private readonly IRepository<UserProgress> _userProgressRepository;
        private readonly IRepository<Lesson> _lessonRepository;
        private readonly IRepository<Course> _courseRepository;

        public UserProgressesController(
            IRepository<UserProgress> userProgressRepository,
            IRepository<Lesson> lessonRepository,
            IRepository<Course> courseRepository)
        {
            _userProgressRepository = userProgressRepository;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? query,
            CancellationToken cancellationToken = default)
        {
            // Get user progress
            var progress = await _userProgressRepository.GetAsync(
                includes: new Expression<Func<UserProgress, object>>[]
                {
                    p => p.User,
                    p => p.Lesson
                },
                tracked: false,
                cancellationToken: cancellationToken
            );

            // Get all lessons
            var lessons = await _lessonRepository.GetAsync(
                tracked: false,
                cancellationToken: cancellationToken
            );

            // Get all courses
            var courses = await _courseRepository.GetAsync(
                tracked: false,
                cancellationToken: cancellationToken
            );

            // Group lessons by course
            var lessonsPerCourse = lessons
                .GroupBy(l => l.CourseId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );

            // Group progress by User + Course
            var result = progress
                .GroupBy(p => new
                {
                    p.UserId,
                    p.User.UserName,
                    CourseId = p.Lesson.CourseId
                })
                .Select(g =>
                {
                    var courseId = g.Key.CourseId;

                    var totalLessons =
                        lessonsPerCourse.TryGetValue(
                            courseId,
                            out var total)
                            ? total
                            : 0;

                    // Distinct lessons to avoid counting
                    // the same lesson twice
                    var completedLessons = g
                        .Where(p => p.IsCompleted)
                        .Select(p => p.LessonId)
                        .Distinct()
                        .Count();

                    var percentage = totalLessons == 0
                        ? 0
                        : (int)Math.Round(
                            completedLessons /
                            (double)totalLessons * 100
                        );

                    var course = courses.FirstOrDefault(
                        c => c.Id == courseId);

                    return new UserProgressVM
                    {
                        UserId = g.Key.UserId,

                        UserName =
                            g.Key.UserName ?? "Unknown User",

                        CourseId = courseId,

                        CourseTitle =
                            course?.Title ?? "Unknown Course",

                        TotalLessons = totalLessons,

                        CompletedLessons = completedLessons,

                        ProgressPercentage = percentage
                    };
                });

            // Search
            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();

                result = result.Where(x =>
                    x.UserName.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    x.CourseTitle.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase)
                );
            }

            var model = result
                .OrderBy(x => x.UserName)
                .ThenBy(x => x.CourseTitle)
                .ToList();

            return View(model);
        }
    }
}
