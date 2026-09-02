using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Training_Platform.Areas.Admin.Controllers
{
    [Area(SD.Admin_Area)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Course> _courseRepository;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
             IRepository<Course> courseRepository,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _courseRepository = courseRepository;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(
    int page = 1,
    string? query = null,
    CancellationToken cancellationToken = default)
        {
            var users = await _userManager.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim().ToLower();

                users = users.Where(u =>
                    u.UserName!.ToLower().Contains(query) ||
                    u.Email!.ToLower().Contains(query))
                    .ToList();
            }
            var users1 = await _userManager.Users.ToListAsync();

            var userRoles = new List<UserWithRoleVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userRoles.Add(new UserWithRoleVM
                {
                    User = user,
                    Role = roles.FirstOrDefault() ?? "No Role"
                });
            }

            int totalPages = (int)Math.Ceiling(users.Count / 6.0);

            users = users
                .Skip((page - 1) * 6)
                .Take(6)
                .ToList();

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
                Roles = await _roleManager.Roles
             .Select(r => new SelectListItem
             {
                 Value = r.Name!,
                 Text = r.Name!
             })
             .ToListAsync()
            };


            return View(vm);
           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVM vm)
        {
            vm.Roles = await _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Name!, Text = r.Name })
                .ToListAsync(); 
            
            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.UserName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                IsApproved = vm.IsApproved,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(vm);
            }

            await _userManager.AddToRoleAsync(user, vm.SelectedRole);

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
                    ModelState.AddModelError("", error.Description);

                return View(vm);
            }

            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var passwordResult = await _userManager.ResetPasswordAsync(
                    user,
                    token,
                    vm.Password);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(vm);
                }
            }

            TempData["success_notification"] = "User updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}