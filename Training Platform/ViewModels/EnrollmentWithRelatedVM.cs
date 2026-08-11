namespace Training_Platform.ViewModels
{
    public class EnrollmentWithRelatedVM
    {
        public IEnumerable<Enrollment> Enrollments { get; set; }
            = Enumerable.Empty<Enrollment>();

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public string? Query { get; set; }
    }
}
