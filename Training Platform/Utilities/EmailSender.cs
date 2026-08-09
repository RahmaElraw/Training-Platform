using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace Training_Platform.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl=true,
                UseDefaultCredentials=false,
                Credentials = new NetworkCredential("rahmaelraw67@gmail.com", "cpdt rgxw xfmp uudq")
            };
            return client.SendMailAsync(
                new MailMessage(
                    from:"rahmaelraw67@gmail.com",
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
