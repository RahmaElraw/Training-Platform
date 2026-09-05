namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            string? query = null,
            CancellationToken cancellationToken = default)
        {
            const int pageSize = 6;

            var users = await _userManager.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();

                users = users
                    .Where(u =>
                        (!string.IsNullOrEmpty(u.UserName) &&
                         u.UserName.Contains(
                             query,
                             StringComparison.OrdinalIgnoreCase))
                        ||
                        (!string.IsNullOrEmpty(u.Email) &&
                         u.Email.Contains(
                             query,
                             StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            if (page < 1)
                page = 1;

            int totalPages = (int)Math.Ceiling(
                users.Count / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var pagedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var userRoles = new List<UserWithRoleVM>();

            foreach (var user in pagedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userRoles.Add(new UserWithRoleVM
                {
                    User = user,
                    Role = roles.FirstOrDefault() ?? "No Role"
                });
            }

            var vm = new UserWithRelatedVM
            {
                Users = userRoles,
                CurrentPage = page,
                TotalPages = totalPages,
                Query = query
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                TempData["error_notification"] =
                    "Failed to delete user.";

                return RedirectToAction(nameof(Index));
            }

            TempData["success_notification"] =
                "User deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.UserName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                IsApproved = vm.IsApproved,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(
                user,
                vm.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(vm);
            }

            TempData["success_notification"] =
                "User created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null)
                return NotFound();

            var vm = new EditUserVM
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                ProfileImage = user.ProfileImage,
                IsApproved = user.IsApproved
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.FindByIdAsync(
                vm.Id.ToString());

            if (user is null)
                return NotFound();

            user.UserName = vm.UserName;
            user.Email = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;
            user.ProfileImage = vm.ProfileImage;
            user.IsApproved = vm.IsApproved;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(vm);
            }
            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var token =
                    await _userManager.GeneratePasswordResetTokenAsync(
                        user);

                var passwordResult =
                    await _userManager.ResetPasswordAsync(
                        user,
                        token,
                        vm.Password);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View(vm);
                }
            }

            TempData["success_notification"] =
                "User updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}