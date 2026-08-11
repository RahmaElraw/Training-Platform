namespace Training_Platform.ViewModels
{
    public class TakeQuizViewModel
    {
        public int QuizId { get; set; }

        public string QuizTitle { get; set; } = string.Empty;

        public List<QuestionViewModel> Questions { get; set; } = new();
    }

    public class QuestionViewModel
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public List<OptionViewModel> Options { get; set; } = new();
    }

    public class OptionViewModel
    {
        public int Id { get; set; }

        public string OptionText { get; set; } = string.Empty;
    }
}
