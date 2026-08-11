namespace Training_Platform.ViewModels
{
    public class CertificateVM
    {
        public int Id { get; set; }

        public string CertificateNumber { get; set; } = string.Empty;

        public DateTime IssueDate { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string CourseTitle { get; set; } = string.Empty;
    }
}
