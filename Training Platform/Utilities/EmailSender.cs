using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace Training_Platform.Utilities
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var gmailEmail = _configuration["EmailSettings:Email"];
            var gmailPassword = _configuration["EmailSettings:AppPassword"];
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl=true,
                UseDefaultCredentials=false,
                Credentials = new NetworkCredential(gmailEmail, gmailPassword)
            };
            return client.SendMailAsync(
                new MailMessage(
                    from: gmailEmail,
                    to:email,
                    subject,
                    htmlMessage
                    )
                    {
                        IsBodyHtml = true
                    });
        }
    }
}
