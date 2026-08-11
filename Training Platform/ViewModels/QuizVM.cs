using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class QuizVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(0, 100)]
        public int PassingScore { get; set; }

        [Required]
        [Range(1, 300)]
        public int TimeLimit { get; set; }

        [Required]
        public int CourseId { get; set; }
    }
}
