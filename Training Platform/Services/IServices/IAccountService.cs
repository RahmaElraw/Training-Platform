using System.Security.Claims;

namespace Training_Platform.Services.IServices
{
    public interface IAccountService
    {
        bool IsLogined(ClaimsPrincipal User);
        Task SendEmailAsync(ApplicationUser user, IUrlHelper url, HttpRequest Request, EmailType emailType = EmailType.ForgotPassword);
    }
}
