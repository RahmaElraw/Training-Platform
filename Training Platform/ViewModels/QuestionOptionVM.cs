using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class QuestionOptionVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string OptionText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int QuestionId { get; set; }
    }
}
