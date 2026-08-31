namespace Training_Platform.ViewModels.Trainee
{
    public class LessonListVM
    {
        public Course Course { get; set; } = null!;
        public List<LessonItemVM> Lessons { get; set; } = new();
        public List<QuizItemVM> Quizzes { get; set; } = new();
    }
    public class QuizItemVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public double BestScorePercentage { get; set; }
        public bool IsPassed { get; set; }
        public bool HasAttempted { get; set; }
        public int? LatestResultId { get; set; }
    }

    public class LessonItemVM
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int OrderNumber { get; set; }

        public bool IsCompleted { get; set; }
    }
}
