using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class CertificateVM
    {
        public int Id { get; set; }

        [Display(Name = "Certificate Number")]
        public string CertificateNumber { get; set; } = string.Empty;

        public DateTime IssueDate { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string CourseTitle { get; set; } = string.Empty;

        [Display(Name = "Certificate Url")]
        public string? CertificateUrl { get; set; }
    }
}
