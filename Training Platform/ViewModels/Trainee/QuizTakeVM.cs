
namespace Training_Platform.ViewModels.Trainee
{
    public class QuizTakeVM
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TimeLimitMinutes { get; set; }
        public int CourseId { get; set; }
        public List<QuestionVM> Questions { get; set; } = new();
    }

    public class QuestionVM
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int Mark { get; set; }
        public QuestionType QuestionType { get; set; }
        public List<OptionVM> Options { get; set; } = new();
    }

    public class OptionVM
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }

    public class QuizSubmitVM
    {
        [Required]
        public int QuizId { get; set; }

        public List<UserAnswerInput> Answers { get; set; } = new();
    }

    public class UserAnswerInput
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }

    public class QuizResultVM
    {
        public int ResultId { get; set; }
        public int QuizId { get; set; } 
        public string QuizTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalPossibleScore { get; set; }
        public int PassingScore { get; set; }
        public bool IsPassed { get; set; }
        public DateTime SubmittedAt { get; set; }
        public int CourseId { get; set; }
    }
}