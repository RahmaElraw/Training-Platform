namespace Training_Platform.ViewModels
{
    public class CertificateWithRelatedVM
    {
        public IEnumerable<Certificate> Certificates { get; set; } = [];

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public string? Query { get; set; }
    }
}
