using System.ComponentModel.DataAnnotations;

namespace Training_Platform.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        public int PassingScore { get; set; }
        [Required]
        public int TimeLimit { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();

        public ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();
    }
}
