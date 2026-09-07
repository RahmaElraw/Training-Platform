namespace Training_Platform.ViewModels.Trainee
{
    public class CourseProgressItemVM
    {
        public string CourseTitle { get; set; } = string.Empty;
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public int ProgressPercentage { get; set; }
    }

    public class TraineeDashboardVM
    {
        public int TotalEnrollments { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public int TotalCertificates { get; set; }
        public List<CourseProgressItemVM> CourseProgresses { get; set; } = [];
        public List<Enrollment> RecentEnrollments { get; set; } = [];
    }
}
