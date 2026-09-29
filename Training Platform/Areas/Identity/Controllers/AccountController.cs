using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using Training_Platform.Areas.Admin.Controllers;

namespace Training_Platform.Areas.Identity.Controllers
{
    [Area(SD.Identity_Area)]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IAccountService _accountService;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOtpRepository;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailSender emailSender,IAccountService accountService,
            IRepository<ApplicationUserOTP> applicationUserOtpRepository,
            IStringLocalizer<SharedResource> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _accountService = accountService;
            _applicationUserOtpRepository = applicationUserOtpRepository;
            _localizer = localizer;
        }





        [HttpGet]
        public IActionResult Login()
        {
            if (_accountService.IsLogined(User))
            {
                if (User.IsInRole(RoleNames.SUPER_ADMIN))
                {
                    return RedirectToAction(nameof(HomeController.Index), SD.Home_Controller, new { area = SD.Admin_Area });
                }

                if (User.IsInRole(RoleNames.TRAINER))
                {
                    return RedirectToAction(nameof(HomeController.Index), SD.Home_Controller, new { area = SD.Trainer_Area });
                }

                if (User.IsInRole(RoleNames.TRAINEE))
                {
                    return RedirectToAction(nameof(HomeController.Index), SD.Home_Controller, new { area = SD.Trainee_Area });
                }

                return RedirectToAction(nameof(Profile));
            }
                return View();
            
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {

           
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }
            var user = await _userManager.FindByEmailAsync(loginVM.EmailOrUserName);//?? await _userManager.FindByNameAsync(loginVM.EmailOrUserName);

            if (user is null)
            {
                var searchTerm = loginVM.EmailOrUserName.Trim();

                user = await _userManager.Users
                    .FirstOrDefaultAsync(u => (u.FirstName + " " + u.LastName) == searchTerm);
            }

            if (user is null)
            {
                ModelState.AddModelError(nameof(loginVM.EmailOrUserName), _localizer["InvalidUsernameOrEmail"]);
                ModelState.AddModelError(nameof(loginVM.Password), _localizer["InvalidPassword"]);
                return View(loginVM);
            }


            if (!user.IsApproved)
            {
                TempData["error"] = _localizer["AccountPendingApproval"].ToString();
                return RedirectToAction(nameof(Login));
            }



            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe,true);

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(nameof(loginVM.EmailOrUserName), _localizer["EmailNotConfirmed"]);
                return View(loginVM);
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(nameof(loginVM.EmailOrUserName), _localizer["InvalidUsernameOrEmail"]);
                ModelState.AddModelError(nameof(loginVM.Password), _localizer["InvalidPassword"]);
                return View(loginVM);
            }
           

            TempData["success"] = _localizer["LoginSuccessfulWelcome"].ToString();

            if (await _userManager.IsInRoleAsync(user, RoleNames.SUPER_ADMIN))
            {
                return RedirectToAction(nameof(HomeController.Index), SD.Home_Controller, new { area = SD.Admin_Area });
            }

            if (await _userManager.IsInRoleAsync(user, RoleNames.TRAINER))
            {
                return RedirectToAction(nameof(HomeController.Index), SD.Home_Controller, new { area = SD.Trainer_Area });
            }

            if (await _userManager.IsInRoleAsync(user, RoleNames.TRAINEE))
            {
                return RedirectToAction(nameof(HomeController.Index), SD.Home_Controller, new { area = SD.Trainee_Area });
            }

            return RedirectToAction(nameof(Profile));
        }

        
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgotPasswordVM forgotPasswordVM)
        {
            if(!ModelState.IsValid)
            {
                return View(forgotPasswordVM);
            }
            var user = await _userManager.FindByEmailAsync(forgotPasswordVM.Email);
            if (user is null)
            {
                ModelState.AddModelError(nameof(forgotPasswordVM.Email), _localizer["EmailNotRegistered"]);
                return View(forgotPasswordVM);
            }

            await _accountService.SendEmailAsync(user, Url, Request, EmailType.ForgotPassword);

            TempData["success"] = _localizer["OtpSentSuccessfully"].ToString();
            TempData["userId"] = user.Id;
            return RedirectToAction(nameof(ValidateOTP));
        }

        [HttpGet]
        public IActionResult ValidateOTP()
        {
            if (TempData.Peek("userId") is null)
            {
                return NotFound();
            }
            if (!int.TryParse(TempData.Peek("userId")?.ToString(), out int userId) || userId == 0)
            {
                return NotFound();
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ValidateOTP(ValidateOTPVM validateOTPVM)
        {
            if (!ModelState.IsValid) return View(validateOTPVM);

            var userIdValue = TempData.Peek("userId")?.ToString();

            if (userIdValue == null)
                return NotFound();

            if (!int.TryParse(userIdValue, out int userId))
                return NotFound();

            if (userId == 0)
                return NotFound();

            var totalOtp = (await _applicationUserOtpRepository.GetAsync(e => e.ApplicationUserId == userId && e.CreateAt >= DateTime.Now.AddHours(-24))).Count();

            if (totalOtp > 3)
            {
                ModelState.AddModelError(nameof(validateOTPVM.OTP), _localizer["OtpAttemptsExceeded"]);
                return View(validateOTPVM);
            }

            var otp = await _applicationUserOtpRepository.GetOneAsync(
                e => e.ApplicationUserId == userId &&
                     e.OTP == validateOTPVM.OTP &&
                     !e.IsUsed &&
                     e.ExpireAt >= DateTime.Now);
            if (otp is null)
            {
                ModelState.AddModelError(nameof(validateOTPVM.OTP), _localizer["InvalidOtp"]);
                return View(validateOTPVM);
            }
            otp.IsUsed = true;
            await _applicationUserOtpRepository.CommitAsync();
            return RedirectToAction(nameof(ResetPassword));
        }
        [HttpPost]
        public async Task<IActionResult> ResendOTP()
        {
            var userIdValue = TempData.Peek("userId")?.ToString();

            if (userIdValue == null)
                return NotFound();

            if (!int.TryParse(userIdValue, out int userId))
                return NotFound();

            if (userId == 0)
                return NotFound();

            var totalOtp = (await _applicationUserOtpRepository.GetAsync(
                    e => e.ApplicationUserId == userId && e.CreateAt >= DateTime.Now.AddHours(-24))).Count();

                if (totalOtp > 3)
                {
                    TempData["error"] = _localizer["OtpAttemptsExceeded"].ToString();
                    return RedirectToAction(nameof(ValidateOTP));
                }

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user is not null)
                {
                    await _accountService.SendEmailAsync(user, Url, Request, EmailType.ForgotPassword);
                }

            TempData["success"] = _localizer["OtpSentSuccessfully"].ToString();
            return RedirectToAction(nameof(ValidateOTP));
        }
        [HttpGet]
        public IActionResult ResetPassword()
        {
            if(TempData.Peek("userId") is null)  return NotFound();
            if (!int.TryParse(TempData.Peek("userId")?.ToString(), out int userId) || userId == 0)
                return NotFound();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVM)
        {
            if (!ModelState.IsValid) return View(resetPasswordVM);

            var userIdValue = TempData.Peek("userId")?.ToString();

            if (userIdValue == null)
                return NotFound();
            if (!int.TryParse(userIdValue, out int userId))
                return NotFound();
            if (userId == 0)
                return NotFound();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user,token, resetPasswordVM.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(resetPasswordVM);
            }

            TempData["success"] = _localizer["PasswordResetSuccessfully"].ToString();
            TempData["userId"] = null;
            return RedirectToAction(nameof(Login));
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser is null)
            {
                return NotFound();
            }

            var user = await _userManager.Users
            .Include(u => u.Enrollments)
            .Include(u => u.Certificates)
            .Include(u => u.Reviews)
            .Include(u => u.CoursesCreated)
            .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

            if (user is null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var profileVM = new ProfileVM
            {
                Email = user.Email!,
                Username = user.UserName!,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                Address = user.Address!,
                PhoneNumber = user.PhoneNumber!,
                ProfileImage = user.ProfileImage,
                CreatedAt = user.CreatedAt,
                Roles = roles,
                EnrollmentsCount = user.Enrollments.Count,
                CertificatesCount = user.Certificates.Count,
                ReviewsCount = user.Reviews.Count,
                CoursesCreatedCount = user.CoursesCreated.Count
            };

            return View(profileVM);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound();
            }

            var profileVM = new ProfileVM
            {
                Email = user.Email!,
                Username = user.UserName!,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                Address = user.Address!,
                PhoneNumber = user.PhoneNumber!,
                ProfileImage = user.ProfileImage
            };

            return View(profileVM);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile(ProfileVM profileVM)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound();
            }

            if (!string.Equals(user.UserName, profileVM.Username, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _userManager.FindByNameAsync(profileVM.Username);
                if (existingUser is not null)
                {
                    ModelState.AddModelError(nameof(profileVM.Username), _localizer["UsernameTaken"]);
                }
            }

            if (!ModelState.IsValid)
            {
                profileVM.Email = user.Email!;
                profileVM.ProfileImage = user.ProfileImage;
                return View(profileVM);
            }

            if (profileVM.ProfileImageFile is not null && profileVM.ProfileImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(profileVM.ProfileImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(profileVM.ProfileImageFile), _localizer["InvalidImageFormat"]);
                    profileVM.Email = user.Email!;
                    profileVM.ProfileImage = user.ProfileImage;
                    return View(profileVM);
                }

                if (profileVM.ProfileImageFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(profileVM.ProfileImageFile), _localizer["ImageSizeLimit"]);
                    profileVM.Email = user.Email!;
                    profileVM.ProfileImage = user.ProfileImage;
                    return View(profileVM);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileVM.ProfileImageFile.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(user.ProfileImage))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles", user.ProfileImage);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.ProfileImage = fileName;
            }

            user.UserName = profileVM.Username;
            user.FirstName = profileVM.FirstName;
            user.LastName = profileVM.LastName;
            user.Address = profileVM.Address;
            user.PhoneNumber = profileVM.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                profileVM.Email = user.Email!;
                profileVM.ProfileImage = user.ProfileImage;
                return View(profileVM);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["success"] = _localizer["ProfileUpdated"].ToString();
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM changePasswordVM)
        {
            if (!ModelState.IsValid)
            {
                return View(changePasswordVM);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordVM.CurrentPassword, changePasswordVM.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(changePasswordVM);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["success"] = _localizer["PasswordChanged"].ToString();
            return RedirectToAction(nameof(Profile));
        }






        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["success"] = _localizer["LoggedOutSuccessfully"].ToString();
            return RedirectToAction(nameof(Login));
        }
       
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}