using System.ComponentModel.DataAnnotations;

namespace Training_Platform.Models
{
    public class QuestionOption
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(500)]
        public string OptionText { get; set; }

        public bool IsCorrect { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
