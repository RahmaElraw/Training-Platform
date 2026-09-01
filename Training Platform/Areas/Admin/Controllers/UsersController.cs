using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(
            int page = 1,
            string? query = null,
            CancellationToken cancellationToken = default)
        {
            const int pageSize = 6;
            var usersQuery = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim().ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.UserName!.ToLower().Contains(query) ||
                    u.Email!.ToLower().Contains(query));
            }

            int totalCount = await usersQuery.CountAsync(cancellationToken);
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedUsers = await usersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

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
        public async Task<IActionResult> ToggleStatus(
            int id,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
                return NotFound();

            user.IsApproved = !user.IsApproved;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = user.IsApproved
                    ? "User activated successfully."
                    : "User deactivated successfully.";
            }
            else
            {
                TempData["Error"] = "Something went wrong.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new CreateUserVM
            {
                Roles = await GetRoleSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVM vm, IFormFile? ProfileImageFile)
        {
            if (!ModelState.IsValid)
            {
                vm.Roles = await GetRoleSelectListAsync();
                return View(vm);
            }

            string? fileName = null;
            if (ProfileImageFile != null && ProfileImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                fileName = $"{Guid.NewGuid()}_{Path.GetFileName(ProfileImageFile.FileName)}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImageFile.CopyToAsync(fileStream);
                }
            }

            var user = new ApplicationUser
            {
                UserName = vm.UserName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                ProfileImage = fileName,
                IsApproved = vm.IsApproved,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                vm.Roles = await GetRoleSelectListAsync();
                return View(vm);
            }

            if (!string.IsNullOrWhiteSpace(vm.SelectedRole))
            {
                await _userManager.AddToRoleAsync(user, vm.SelectedRole);
            }

            TempData["success_notification"] = "User created successfully.";
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

            var user = await _userManager.FindByIdAsync(vm.Id.ToString());

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
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(vm);
            }

            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, vm.Password);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View(vm);
                }
            }

            TempData["success_notification"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetRoleSelectListAsync()
        {
            return await _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name!,
                    Text = r.Name
                })
                .ToListAsync();
        }
    }
}