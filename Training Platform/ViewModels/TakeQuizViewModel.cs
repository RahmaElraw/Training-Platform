using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class TakeQuizViewModel
    {
        [Display(Name = "Quiz Id")]
        public int QuizId { get; set; }

        public string QuizTitle { get; set; } = string.Empty;

        public List<QuestionViewModel> Questions { get; set; } = new();
    }

    public class QuestionViewModel
    {
        public int QuestionId { get; set; }

        [Display(Name = "Question Text")]
        public string QuestionText { get; set; } = string.Empty;

        public List<OptionViewModel> Options { get; set; } = new();
    }

    public class OptionViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Option Text")]
        public string OptionText { get; set; } = string.Empty;
    }
}
