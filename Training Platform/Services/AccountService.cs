using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Security.Claims;
using Training_Platform.Areas.Identity.Controllers;

namespace Training_Platform.Services
{
   public enum EmailType
    {
        Register,
        ResendConfirmation,
        ForgotPassword
    }
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOtpRepository;
        public AccountService(UserManager<ApplicationUser> userManager, IEmailSender emailSender,
             IRepository<ApplicationUserOTP> applicationUserOtpRepository)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _applicationUserOtpRepository = applicationUserOtpRepository;
        }
        public bool IsLogined(ClaimsPrincipal User)
        {
            if (User is not null && User.Identity.IsAuthenticated)
            {
                return true;
            }
            return false;

        }

        public async Task SendEmailAsync(ApplicationUser user, IUrlHelper url, HttpRequest Request,EmailType emailType = EmailType.Register)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = url.Action(nameof(AccountController.Confirm), SD.Account_Controller, new { area = SD.Identity_Area, token, userId = user.Id }, Request.Scheme);
           
            string subject = string.Empty;
            string message = string.Empty;
            switch (emailType)
            {
                case EmailType.Register:
                    {
                        subject = "Confirm your email";
                        message = $"Please confirm your account by clicking this link: <a href='{link}'>Confirm Email</a>";
                    }
                    break;

                case EmailType.ResendConfirmation:
                    {
                        subject = "Resend email confirmation";
                        message = $"Please confirm your account by clicking this link: <a href='{link}'>Confirm Email</a>";
                    }
                    break;
                case EmailType.ForgotPassword:
                    {
                        var otp = new Random().Next(1000, 9999).ToString();
                      await _applicationUserOtpRepository.AddAsync(new()
                        {
                            OTP=otp,
                            ApplicationUserId= user.Id,
                        });
                        await _applicationUserOtpRepository.CommitAsync();
                        subject = "Reset your password";
                        message = $"Use this Otp: {otp} to reset your password.";
                    }
                    break;
            }

            await _emailSender.SendEmailAsync(user.Email!, subject, message);
        }
    }
}
