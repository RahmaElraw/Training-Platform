using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Training_Platform.Models;

namespace Training_Platform.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly IWebHostEnvironment _environment;

        public CertificateService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public Task<string> GenerateCertificateAsync(
            Certificate certificate,
            ApplicationUser user,
            Course course,
            CancellationToken cancellationToken = default)
        {
            var certificatesFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "certificates");

            Directory.CreateDirectory(certificatesFolder);

            var fileName =
                $"{certificate.CertificateNumber}.pdf";

            var filePath =
                Path.Combine(
                    certificatesFolder,
                    fileName);

            var traineeName =
                $"{user.FirstName} {user.LastName}".Trim();

            if (string.IsNullOrWhiteSpace(traineeName))
            {
                traineeName = user.Email ?? "Trainee";
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());

                    page.Margin(40);

                    page.PageColor(Colors.White);

                    page.Content()
                        .Border(4)
                        .BorderColor(Colors.Blue.Medium)
                        .Padding(40)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            column.Item()
                                .AlignCenter()
                                .Text("CERTIFICATE OF COMPLETION")
                                .FontSize(30)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item()
                                .AlignCenter()
                                .Text("This certificate is proudly presented to")
                                .FontSize(16);

                            column.Item()
                                .PaddingTop(15)
                                .AlignCenter()
                                .Text(traineeName)
                                .FontSize(28)
                                .Bold();

                            column.Item()
                                .AlignCenter()
                                .Text("for successfully completing the course")
                                .FontSize(16);

                            column.Item()
                                .PaddingTop(10)
                                .AlignCenter()
                                .Text(course.Title)
                                .FontSize(24)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item()
                                .PaddingTop(20)
                                .AlignCenter()
                                .Text(
                                    $"Issued on {certificate.IssueDate:dd MMMM yyyy}")
                                .FontSize(14);

                            column.Item()
                                .PaddingTop(10)
                                .AlignCenter()
                                .Text(
                                    $"Certificate Number: {certificate.CertificateNumber}")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken2);

                            column.Item()
                                .PaddingTop(25)
                                .AlignCenter()
                                .Text("Training Platform")
                                .FontSize(16)
                                .Bold();
                        });
                });
            });

            document.GeneratePdf(filePath);

            var certificateUrl =
                $"/certificates/{fileName}";

            return Task.FromResult(certificateUrl);
        }
    }
}