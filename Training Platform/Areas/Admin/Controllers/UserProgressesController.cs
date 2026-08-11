using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    public class UserProgressesController : Controller
    {
        private readonly IRepository<UserProgress> _userProgressRepository;
        private readonly IRepository<ApplicationUser> _userRepository;
        private readonly IRepository<Lesson> _lessonRepository;

        private const int PageSize = 6;

        public UserProgressesController(
            IRepository<UserProgress> userProgressRepository,
            IRepository<ApplicationUser> userRepository,
            IRepository<Lesson> lessonRepository)
        {
            _userProgressRepository = userProgressRepository;
            _userRepository = userRepository;
            _lessonRepository = lessonRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(
            string? query,
            int page = 1)
        {
            var progresses =
                await _userProgressRepository.GetAsync(
                    includes:
                    [
                        p => p.User,
                        p => p.Lesson,
                        p => p.Lesson.Course
                    ],
                    tracked: false);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();

                progresses = progresses.Where(p =>
                    (p.User.UserName != null &&
                     p.User.UserName.Contains(
                         query,
                         StringComparison.OrdinalIgnoreCase))
                    ||
                    p.Lesson.Title.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase));
            }

            progresses = progresses
                .OrderBy(p => p.User.UserName)
                .ThenBy(p => p.Lesson.OrderNumber);

            int totalCount = progresses.Count();

            var model = new UserProgressWithRelatedVM
            {
                UserProgresses = progresses
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize),

                CurrentPage = page,

                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)PageSize),

                Query = query
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadProgressData();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserProgressVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadProgressData(model);
                return View(model);
            }
            var user = await _userRepository.GetOneAsync(
                u => u.Id == model.UserId);

            if (user == null)
            {
                ModelState.AddModelError(
                    nameof(model.UserId),
                    "Selected user does not exist.");

                await LoadProgressData(model);
                return View(model);
            }
            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == model.LessonId);

            if (lesson == null)
            {
                ModelState.AddModelError(
                    nameof(model.LessonId),
                    "Selected lesson does not exist.");

                await LoadProgressData(model);
                return View(model);
            }
            var exists =
                await _userProgressRepository.GetOneAsync(
                    p => p.UserId == model.UserId
                         && p.LessonId == model.LessonId);

            if (exists != null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Progress for this user and lesson already exists.");

                await LoadProgressData(model);
                return View(model);
            }


            var progress = new UserProgress
            {
                UserId = model.UserId,

                LessonId = model.LessonId,

                IsCompleted = model.IsCompleted,

                CompletedAt = model.IsCompleted
                    ? DateTime.UtcNow
                    : null
            };


            await _userProgressRepository.AddAsync(progress);


            if (await _userProgressRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "User progress created successfully.";

                return RedirectToAction(nameof(Index));
            }


            TempData["Error"] =
                "Something went wrong.";

            await LoadProgressData(model);

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var progress =
                await _userProgressRepository.GetOneAsync(
                    p => p.Id == id);

            if (progress == null)
                return NotFound();


            var model = new UserProgressVM
            {
                Id = progress.Id,

                UserId = progress.UserId,

                LessonId = progress.LessonId,

                IsCompleted = progress.IsCompleted,

                CompletedAt = progress.CompletedAt
            };


            await LoadProgressData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProgressVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadProgressData(model);
                return View(model);
            }


            var progress =
                await _userProgressRepository.GetOneAsync(
                    p => p.Id == model.Id);

            if (progress == null)
                return NotFound();

            var user = await _userRepository.GetOneAsync(
                u => u.Id == model.UserId);

            if (user == null)
            {
                ModelState.AddModelError(
                    nameof(model.UserId),
                    "Selected user does not exist.");

                await LoadProgressData(model);
                return View(model);
            }
            var lesson = await _lessonRepository.GetOneAsync(
                l => l.Id == model.LessonId);

            if (lesson == null)
            {
                ModelState.AddModelError(
                    nameof(model.LessonId),
                    "Selected lesson does not exist.");

                await LoadProgressData(model);
                return View(model);
            }
            var exists =
                await _userProgressRepository.GetOneAsync(
                    p => p.UserId == model.UserId
                         && p.LessonId == model.LessonId
                         && p.Id != model.Id);

            if (exists != null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Progress for this user and lesson already exists.");

                await LoadProgressData(model);
                return View(model);
            }


            progress.UserId =
                model.UserId;

            progress.LessonId =
                model.LessonId;

            progress.IsCompleted =
                model.IsCompleted;

            if (model.IsCompleted)
            {
                progress.CompletedAt =
                    progress.CompletedAt
                    ?? DateTime.UtcNow;
            }
            else
            {
                progress.CompletedAt = null;
            }


            _userProgressRepository.Update(progress);


            if (await _userProgressRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "User progress updated successfully.";

                return RedirectToAction(nameof(Index));
            }


            TempData["Error"] =
                "Something went wrong.";

            await LoadProgressData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var progress =
                await _userProgressRepository.GetOneAsync(
                    p => p.Id == id);

            if (progress == null)
                return NotFound();


            _userProgressRepository.Delete(progress);


            if (await _userProgressRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "User progress deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Something went wrong.";
            }


            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var progress =
                await _userProgressRepository.GetOneAsync(
                    p => p.Id == id,
                    includes:
                    [
                        p => p.User,
                        p => p.Lesson,
                        p => p.Lesson.Course
                    ]);


            if (progress == null)
                return NotFound();


            return View(progress);
        }
        private async Task LoadProgressData(
            UserProgressVM? model = null)
        {
            var users =
                await _userRepository.GetAsync(
                    tracked: false);

            var lessons =
                await _lessonRepository.GetAsync(
                    includes:
                    [
                        l => l.Course
                    ],
                    tracked: false);


            ViewBag.Users =
                new SelectList(
                    users.OrderBy(u => u.UserName),
                    "Id",
                    "UserName",
                    model?.UserId);


            ViewBag.Lessons =
                new SelectList(
                    lessons
                        .OrderBy(l => l.Course.Title)
                        .ThenBy(l => l.OrderNumber)
                        .Select(l => new
                        {
                            l.Id,
                            DisplayName =
                                $"{l.Course.Title} - {l.OrderNumber}. {l.Title}"
                        }),
                    "Id",
                    "DisplayName",
                    model?.LessonId);
        }
    }
}
