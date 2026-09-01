namespace Training_Platform.ViewModels.Trainee
{
    public class CourseDetailsVM
    {
        public Course Course { get; set; } = null!;
        public bool IsEnrolled { get; set; }
        public bool IsCompleted { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int UserRating { get; set; }
        public string? UserComment { get; set; }
    }
}
