using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class QuestionOptionVM
    {
        public int Id { get; set; }

        
        [MaxLength(500)]
        public string? OptionText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        
        public int QuestionId { get; set; }
    }
}
