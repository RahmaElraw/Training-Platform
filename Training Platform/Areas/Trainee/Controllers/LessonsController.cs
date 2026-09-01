using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Training_Platform.ViewModels.Trainee;

namespace Training_Platform.Areas.Trainee.Controllers
{
    [Area(SD.Trainee_Area)]
    [Authorize]
    public class LessonsController : Controller
    {
        private readonly IRepository<Lesson> _lessonRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Enrollment> _enrollmentRepository;
        private readonly IRepository<UserProgress> _progressRepository;
        private readonly IRepository<Quiz> _quizRepository;
        private readonly IRepository<QuizResult> _quizResultRepository;

        public LessonsController(
            IRepository<Lesson> lessonRepository,
            IRepository<Course> courseRepository,
            IRepository<Enrollment> enrollmentRepository,
            IRepository<UserProgress> progressRepository,
            IRepository<Quiz> quizRepository,
            IRepository<QuizResult> quizResultRepository)
        {
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _progressRepository = progressRepository;
            _quizRepository = quizRepository;
            _quizResultRepository = quizResultRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int courseId,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == courseId && c.IsPublished,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (course is null)
                return NotFound();

            var enrollment = await _enrollmentRepository.GetOneAsync(
                e => e.UserId == userId && e.CourseId == courseId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (enrollment is null)
            {
                TempData["Error"] = "You must enroll in this course first.";
                return RedirectToAction("Details", "Courses", new { area = SD.Trainee_Area, id = courseId });
            }

            // 1. Lessons & Progress
            var lessons = await _lessonRepository.GetAsync(
                l => l.CourseId == courseId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            var lessonList = lessons.OrderBy(l => l.OrderNumber).ToList();
            var lessonIds = lessonList.Select(l => l.Id).ToList();

            var progresses = await _progressRepository.GetAsync(
                p => p.UserId == userId && lessonIds.Contains(p.LessonId),
                tracked: false,
                cancellationToken: cancellationToken
            );

            var completedLessonIds = progresses
                .Where(p => p.IsCompleted)
                .Select(p => p.LessonId)
                .ToHashSet();

            // Fetch quizzes with Questions included
            var quizzes = await _quizRepository.GetAsync(
                q => q.CourseId == courseId,
                includes: [q => q.Questions],
                tracked: false,
                cancellationToken: cancellationToken
            );

            var quizIds = quizzes.Select(q => q.Id).ToList();

            var results = await _quizResultRepository.GetAsync(
                r => r.UserId == userId && quizIds.Contains(r.QuizId),
                tracked: false,
                cancellationToken: cancellationToken
            );

            var quizListVM = quizzes.Select(q =>
            {
                var qResults = results.Where(r => r.QuizId == q.Id).ToList();
                var latestResult = qResults.OrderByDescending(r => r.SubmittedAt).FirstOrDefault();
                var bestResult = qResults.OrderByDescending(r => r.Score).FirstOrDefault();

                int totalPossibleMarks = (q.Questions != null && q.Questions.Any())
                    ? q.Questions.Sum(quest => quest.Mark)
                    : 0;

                double scorePercentage = 0;
                if (bestResult != null && totalPossibleMarks > 0)
                {
                    scorePercentage = Math.Round(((double)bestResult.Score / totalPossibleMarks) * 100, 1);
                }

                return new QuizItemVM
                {
                    Id = q.Id,
                    Title = q.Title,
                    BestScorePercentage = scorePercentage,
                    HasAttempted = qResults.Any(),
                    IsPassed = qResults.Any(r => r.IsPassed),
                    LatestResultId = latestResult?.Id
                };
            }).ToList();

            var vm = new LessonListVM
            {
                Course = course,
                Lessons = lessonList.Select(l => new LessonItemVM
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    OrderNumber = l.OrderNumber,
                    IsCompleted = completedLessonIds.Contains(l.Id)
                }).ToList(),
                Quizzes = quizListVM
            };

            return View(vm);
        }
        public static class UrlHelperExtensions
        {
            public static string ToEmbedUrl(string? url)
            {
                if (string.IsNullOrWhiteSpace(url)) return string.Empty;

                // Standard YouTube URL: https://www.youtube.com/watch?v=VIDEO_ID
                if (url.Contains("youtube.com/watch"))
                {
                    var uri = new Uri(url);
                    var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    var videoId = queryParams["v"];
                    return $"https://www.youtube.com/embed/{videoId}";
                }

                // Shortened YouTube URL: https://youtu.be/VIDEO_ID
                if (url.Contains("youtu.be/"))
                {
                    var videoId = url.Split('/').Last().Split('?').First();
                    return $"https://www.youtube.com/embed/{videoId}";
                }

                // YouTube Shorts: https://www.youtube.com/shorts/VIDEO_ID
                if (url.Contains("youtube.com/shorts/"))
                {
                    var videoId = url.Split('/').Last().Split('?').First();
                    return $"https://www.youtube.com/embed/{videoId}";
                }

                return url;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == id,
                includes:
                [
                    l => l.Course,
                    l => l.CourseMaterials
                ],
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (lesson is null)
                return NotFound();

            // Make sure trainee is enrolled in this course
            var enrollment = await _enrollmentRepository.GetOneAsync(
                e => e.UserId == userId &&
                     e.CourseId == lesson.CourseId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            if (enrollment is null)
            {
                TempData["Error"] =
                    "You must enroll in this course first.";

                return RedirectToAction(
                    "Details",
                    "Courses",
                    new
                    {
                        area = SD.Trainee_Area,
                        id = lesson.CourseId
                    });
            }

            // Get current progress
            var progress = await _progressRepository.GetOneAsync(
                p => p.UserId == userId &&
                     p.LessonId == lesson.Id,
                tracked: false,
                cancellationToken: cancellationToken
            );

            // Get all lessons to determine previous 
            var courseLessons = await _lessonRepository.GetAsync(
                l => l.CourseId == lesson.CourseId,
                tracked: false,
                cancellationToken: cancellationToken
            );

            var orderedLessons = courseLessons
                .OrderBy(l => l.OrderNumber)
                .ToList();

            var currentIndex = orderedLessons
                .FindIndex(l => l.Id == lesson.Id);

            int? previousLessonId = null;
            int? nextLessonId = null;

            if (currentIndex > 0)
            {
                previousLessonId =
                    orderedLessons[currentIndex - 1].Id;
            }

            if (currentIndex >= 0 &&
                currentIndex < orderedLessons.Count - 1)
            {
                nextLessonId =
                    orderedLessons[currentIndex + 1].Id;
            }

            var vm = new LessonDetailsVM
            {
                Lesson = lesson,
                Course = lesson.Course,
                IsCompleted = progress?.IsCompleted ?? false,
                PreviousLessonId = previousLessonId,
                NextLessonId = nextLessonId
            };

            vm.Lesson.VideoUrl = UrlHelperExtensions.ToEmbedUrl(lesson.VideoUrl);

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(
            int lessonId,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            // Get Lesson
            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == lessonId,
                tracked: false,
                cancellationToken: cancellationToken);

            if (lesson is null)
                return NotFound();

            // Check Enrollment
            var enrollment = await _enrollmentRepository.GetOneAsync(
                e => e.UserId == userId &&
                     e.CourseId == lesson.CourseId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (enrollment is null)
                return Unauthorized();

            // Check Existing Progress
            var progress = await _progressRepository.GetOneAsync(
                p => p.UserId == userId &&
                     p.LessonId == lessonId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (progress is null)
            {
                progress = new UserProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };

                await _progressRepository.AddAsync(
                    progress,
                    cancellationToken);
            }
            else
            {
                progress.IsCompleted = true;
                progress.CompletedAt = DateTime.UtcNow;
            }

            await _progressRepository.CommitAsync(
                cancellationToken);

            // Check Course Completion
            await CheckCourseCompletion(
                userId,
                lesson.CourseId,
                cancellationToken);

            TempData["Success"] =
                "Lesson completed successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = lessonId });
        }
        private async Task CheckCourseCompletion(
            int userId,
            int courseId,
            CancellationToken cancellationToken)
        {
            // 1. Verify all lessons completed
            var lessons = await _lessonRepository.GetAsync(
                l => l.CourseId == courseId,
                tracked: false,
                cancellationToken: cancellationToken);

            var totalLessonsCount = lessons.Count();
            bool areLessonsComplete = true;

            if (totalLessonsCount > 0)
            {
                var lessonIds = lessons.Select(l => l.Id).ToList();
                var completedLessonsCount = (await _progressRepository.GetAsync(
                    p => p.UserId == userId && p.IsCompleted && lessonIds.Contains(p.LessonId),
                    tracked: false,
                    cancellationToken: cancellationToken))
                    .Select(p => p.LessonId).Distinct().Count();

                areLessonsComplete = (completedLessonsCount == totalLessonsCount);
            }

            // 2. Verify all quizzes passed with Score >= 70%
            var quizzes = await _quizRepository.GetAsync(
                q => q.CourseId == courseId,
                tracked: false,
                cancellationToken: cancellationToken);

            var totalQuizzesCount = quizzes.Count();
            bool areQuizzesComplete = true;

            if (totalQuizzesCount > 0)
            {
                var quizIds = quizzes.Select(q => q.Id).ToList();
                var results = await _quizResultRepository.GetAsync(
                    r => r.UserId == userId && quizIds.Contains(r.QuizId),
                    tracked: false,
                    cancellationToken: cancellationToken);

                foreach (var quiz in quizzes)
                {
                    var maxScore = results
                        .Where(r => r.QuizId == quiz.Id)
                        .Select(r => r.Score)
                        .DefaultIfEmpty(0)
                        .Max();

                    if (maxScore < 70)
                    {
                        areQuizzesComplete = false;
                        break;
                    }
                }
            }

            // 3. Update Enrollment Completion Status
            var enrollment = await _enrollmentRepository.GetOneAsync(
                e => e.UserId == userId && e.CourseId == courseId,
                tracked: true,
                cancellationToken: cancellationToken);

            if (enrollment is not null)
            {
                bool isNowCompleted = areLessonsComplete && areQuizzesComplete;

                if (enrollment.IsCompleted != isNowCompleted)
                {
                    enrollment.IsCompleted = isNowCompleted;
                    await _enrollmentRepository.CommitAsync(cancellationToken);
                }
            }
        }

        private int GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException();

            return int.Parse(userId);
        }
    }
}