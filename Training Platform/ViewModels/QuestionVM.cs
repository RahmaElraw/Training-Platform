using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class QuestionVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [Range(1, 100)]
        public int Mark { get; set; }

        [Required]
        public QuestionType QuestionType { get; set; }

        [Required]
        public int QuizId { get; set; }
        public List<QuestionOptionVM> QuestionOptions { get; set; }
            = new List<QuestionOptionVM>();
    }
}
