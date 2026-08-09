using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
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
        public AccountController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager, IEmailSender emailSender,IAccountService accountService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _accountService = accountService;

        }


        [HttpGet]
        public IActionResult Register()
        { 
            if (_accountService.IsLogined(User))
            {
                return RedirectToAction(nameof(HomeController.Index), SD.Home_Controller, new { area = SD.Admin_Area });
            }


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }
            ApplicationUser user = new()
            {
                UserName = registerVM.Username,
                Email = registerVM.Email,
                FirstName = registerVM.FName,
                LastName = registerVM.LName,
                Address = registerVM.Address,
            };
            var result = await _userManager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                    return View(registerVM);
                }
            }
            // send email confirmation 
            await _accountService.SendEmailAsync(user, Url, Request);


            TempData["success"] = "Account created successfully. Please check your email to confirm your account.";

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Confirm(string token, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                TempData["error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction("Login");
            }
            TempData["success"] = "Email confirmed successfully. You can now log in.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_accountService.IsLogined(User))
            {
                return RedirectToAction(nameof(HomeController.Index), SD.Home_Controller, new { area = SD.Admin_Area });
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
           var user = await _userManager.FindByEmailAsync(loginVM.EmailOrUserName) ?? await _userManager.FindByNameAsync(loginVM.EmailOrUserName);

           if(user is null)
            {
                ModelState.AddModelError(nameof(loginVM.EmailOrUserName), "Invalid Username or Email.");
                ModelState.AddModelError(nameof(loginVM.Password), "Invalid Password.");
                return View(loginVM);
            }



           var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe,true);

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(nameof(loginVM.EmailOrUserName), "Email not confirmed. Please check your email for confirmation link.");
                return View(loginVM);
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(nameof(loginVM.EmailOrUserName), "Invalid Username or Email.");
                ModelState.AddModelError(nameof(loginVM.Password), "Invalid Password.");
                return View(loginVM);
            }
           

            TempData["success"] = $"Login successful. Welcome, {user.FirstName} {user.LastName}";

            return RedirectToAction(nameof(HomeController.Index), SD.Home_Controller, new {area= SD.Admin_Area} );
        }

        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            if (!ModelState.IsValid)
            {
                return View(resendEmailConfirmationVM);
            }
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationVM.EmailOrUserName) ?? await _userManager.FindByNameAsync(resendEmailConfirmationVM.EmailOrUserName);

           if(user is not null && !user.EmailConfirmed)
            {
                await _accountService.SendEmailAsync(user, Url, Request, EmailType.ResendConfirmation);
            }
           TempData["success"] = "Resend confirmation email sent successfully. Please check your email.";
            return RedirectToAction(nameof(Login));
        }



        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }
       
    }
}