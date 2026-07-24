using System.ComponentModel.DataAnnotations;

namespace Training_Platform.Models
{
    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse
    }

    public class Question
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(500)]
        public string QuestionText { get; set; }
        [Required]
        public int Mark { get; set; }
        [Required]
        public QuestionType QuestionType { get; set; }

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        public ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();
    }
}
