using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    public class EnrollmentsController : Controller
    {
        private readonly IRepository<Enrollment> _enrollmentRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<ApplicationUser> _userRepository;

        private const int PageSize = 6;

        public EnrollmentsController(
            IRepository<Enrollment> enrollmentRepository,
            IRepository<Course> courseRepository,
            IRepository<ApplicationUser> userRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(
            string? query,
            int page = 1)
        {
            var enrollments =
                await _enrollmentRepository.GetAsync(
                    includes:
                    [
                        e => e.User,
                        e => e.Course
                    ],
                    tracked: false);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();

                enrollments = enrollments.Where(e =>
                    e.User.UserName != null &&
                    e.User.UserName.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    e.Course.Title.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase));
            }

            enrollments = enrollments
                .OrderByDescending(e => e.EnrollmentDate);

            int totalCount = enrollments.Count();

            var model = new EnrollmentWithRelatedVM
            {
                Enrollments = enrollments
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
            await LoadEnrollmentData();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnrollmentVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadEnrollmentData(model);
                return View(model);
            }
            var user = await _userRepository.GetOneAsync(
                u => u.Id == model.UserId);

            if (user == null)
            {
                ModelState.AddModelError(
                    nameof(model.UserId),
                    "Selected user does not exist.");

                await LoadEnrollmentData(model);
                return View(model);
            }

            var course = await _courseRepository.GetOneAsync(
                c => c.Id == model.CourseId);

            if (course == null)
            {
                ModelState.AddModelError(
                    nameof(model.CourseId),
                    "Selected course does not exist.");

                await LoadEnrollmentData(model);
                return View(model);
            }

            var exists = await _enrollmentRepository.GetOneAsync(
                e => e.UserId == model.UserId
                     && e.CourseId == model.CourseId);

            if (exists != null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This user is already enrolled in this course.");

                await LoadEnrollmentData(model);
                return View(model);
            }


            var enrollment = new Enrollment
            {
                EnrollmentDate = DateTime.UtcNow,

                UserId = model.UserId,

                CourseId = model.CourseId,

                IsCompleted = false
            };


            await _enrollmentRepository.AddAsync(enrollment);


            if (await _enrollmentRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Enrollment created successfully.";

                return RedirectToAction(nameof(Index));
            }


            TempData["Error"] =
                "Something went wrong.";

            await LoadEnrollmentData(model);

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var enrollment = await _enrollmentRepository.GetOneAsync(
                e => e.Id == id);

            if (enrollment == null)
                return NotFound();

            var model = new EnrollmentVM
            {
                Id = enrollment.Id,
                UserId = enrollment.UserId,
                CourseId = enrollment.CourseId
            };

            await LoadEnrollmentData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EnrollmentVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadEnrollmentData(model);
                return View(model);
            }
            var enrollment =
                await _enrollmentRepository.GetOneAsync(
                    e => e.Id == model.Id);

            if (enrollment == null)
                return NotFound();

            var user = await _userRepository.GetOneAsync(
                u => u.Id == model.UserId);

            if (user == null)
            {
                ModelState.AddModelError(
                    nameof(model.UserId),
                    "Selected user does not exist.");

                await LoadEnrollmentData(model);
                return View(model);
            }
            var course = await _courseRepository.GetOneAsync(
                c => c.Id == model.CourseId);

            if (course == null)
            {
                ModelState.AddModelError(
                    nameof(model.CourseId),
                    "Selected course does not exist.");

                await LoadEnrollmentData(model);
                return View(model);
            }
            var exists =
                await _enrollmentRepository.GetOneAsync(
                    e => e.UserId == model.UserId
                         && e.CourseId == model.CourseId
                         && e.Id != model.Id);

            if (exists != null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This user is already enrolled in this course.");

                await LoadEnrollmentData(model);
                return View(model);
            }

            enrollment.UserId =
                model.UserId;

            enrollment.CourseId =
                model.CourseId;

            _enrollmentRepository.Update(enrollment);


            if (await _enrollmentRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Enrollment updated successfully.";

                return RedirectToAction(nameof(Index));
            }


            TempData["Error"] =
                "Something went wrong.";

            await LoadEnrollmentData(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var enrollment =
                await _enrollmentRepository.GetOneAsync(
                    e => e.Id == id);

            if (enrollment == null)
                return NotFound();


            _enrollmentRepository.Delete(enrollment);


            if (await _enrollmentRepository.CommitAsync() > 0)
            {
                TempData["Success"] =
                    "Enrollment deleted successfully.";
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
            var enrollment =
                await _enrollmentRepository.GetOneAsync(
                    e => e.Id == id,
                    includes:
                    [
                        e => e.User,
                        e => e.Course
                    ]);


            if (enrollment == null)
                return NotFound();


            return View(enrollment);
        }
        private async Task LoadEnrollmentData(
            EnrollmentVM? model = null)
        {
            var users =
                await _userRepository.GetAsync(
                    tracked: false);

            var courses =
                await _courseRepository.GetAsync(
                    tracked: false);


            ViewBag.Users =
                new SelectList(
                    users.OrderBy(u => u.UserName),
                    "Id",
                    "UserName",
                    model?.UserId);


            ViewBag.Courses =
                new SelectList(
                    courses.OrderBy(c => c.Title),
                    "Id",
                    "Title",
                    model?.CourseId);
        }
    }
}
