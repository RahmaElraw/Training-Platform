namespace Training_Platform.Areas.Trainer.ViewModels
{
    public class TrainerHomeVM
    {
        // Main Statistics
        public int MyCourses { get; set; }
        public int PublishedCourses { get; set; }
        public int MyQuizzes { get; set; }
        public int TotalTrainees { get; set; }

        // Progress Statistics
        public int CompletedEnrollments { get; set; }
        public int InProgressEnrollments { get; set; }

        // Certificates
        public int CertificatesIssued { get; set; }

        // Charts
        public List<CourseStatisticVM> CourseStatistics { get; set; }
            = new();

        // Recent Courses
        public List<RecentCourseVM> RecentCourses { get; set; }
            = new();
    }

    public class CourseStatisticVM
    {
        public string CourseTitle { get; set; } = string.Empty;

        public int TraineesCount { get; set; }

        public int CompletedCount { get; set; }

        public int InProgressCount { get; set; }
    }

    public class RecentCourseVM
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        public DateTime CreatedAt { get; set; }

        public int TraineesCount { get; set; }
    }
}