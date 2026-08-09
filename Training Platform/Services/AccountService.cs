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
    }
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AccountService(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
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
            }

            await _emailSender.SendEmailAsync(user.Email!, subject, message);
        }
    }
}
