namespace Training_Platform.ViewModels
{
    public class QuizResultViewModel
    {
        public string QuizTitle { get; set; } = string.Empty;

        public int Score { get; set; }

        public bool IsPassed { get; set; }

        public DateTime SubmittedAt { get; set; }
    }
}
