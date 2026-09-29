namespace Training_Platform.ViewModels
{
    public class TrainerProgressVM
    {
        public int EnrollmentId { get; set; }

        public int UserId { get; set; }

        public string TraineeName { get; set; } = string.Empty;

        public string? TraineeEmail { get; set; }

        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;

        public int TotalLessons { get; set; }

        public int CompletedLessons { get; set; }

        public double ProgressPercentage { get; set; }

        public bool IsCompleted { get; set; }
    }
}