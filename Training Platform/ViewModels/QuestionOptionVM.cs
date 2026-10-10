using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class QuestionOptionVM
    {
        public int Id { get; set; }

        
        [MaxLength(500)]
        [Display(Name = "Option Text")]
        public string? OptionText { get; set; } = string.Empty;

        [Display(Name = "Is Correct")]
        public bool IsCorrect { get; set; }

        
        public int QuestionId { get; set; }
    }
}
