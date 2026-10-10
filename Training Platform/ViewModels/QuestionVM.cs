using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class QuestionVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        [Display(Name = "Question Text")]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [Range(1, 100)]
        [Display(Name = "Mark")]
        public int Mark { get; set; }

        [Required]
        [Display(Name = "Question Type")]
        public QuestionType QuestionType { get; set; }

        [Required]
        [Display(Name = "Quiz Id")]
        public int QuizId { get; set; }
        public List<QuestionOptionVM> QuestionOptions { get; set; }
            = new List<QuestionOptionVM>();
    }
}
