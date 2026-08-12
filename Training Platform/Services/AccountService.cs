using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Security.Claims;

namespace Training_Platform.Services
{
    public enum EmailType
    {
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

        public async Task SendEmailAsync(ApplicationUser user, IUrlHelper url, HttpRequest Request, EmailType emailType = EmailType.ForgotPassword)
        {
            string subject = string.Empty;
            string message = string.Empty;

            switch (emailType)
            {
                case EmailType.ForgotPassword:
                    {
                        var otp = new Random().Next(1000, 9999).ToString();
                        await _applicationUserOtpRepository.AddAsync(new()
                        {
                            OTP = otp,
                            ApplicationUserId = user.Id,
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