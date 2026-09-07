namespace Training_Platform.ViewModels.Trainee
{
    public class EnrollmentItemVM
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string? CourseThumbnail { get; set; }
        public string Level { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public bool IsCompleted { get; set; }
        public int TotalLessons { get; set; }

        public bool HasReviewed { get; set; }
        public int? ReviewId { get; set; }
        public int? QuizId { get; set; }
        public bool HasQuiz => QuizId.HasValue;
    }
}
