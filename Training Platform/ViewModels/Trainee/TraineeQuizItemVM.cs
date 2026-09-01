namespace Training_Platform.ViewModels.Trainee
{
    public class TraineeQuizItemVM
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int TimeLimitMinutes { get; set; }
        public int PassingScore { get; set; }
        public int QuestionCount { get; set; }
        public int TotalPossibleScore { get; set; }
        public bool HasAttempted { get; set; }
        public int? BestScore { get; set; }
        public bool IsPassed { get; set; }
        public DateTime? LastAttemptDate { get; set; }
    }
}
