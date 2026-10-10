using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class QuizVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(0, 100)]
        [Display(Name = "Passing Score")]
        public int PassingScore { get; set; }

        [Required]
        [Range(1, 300)]
        [Display(Name = "Time Limit")]
        public int TimeLimit { get; set; }

        [Required]
        [Display(Name = "Course Id")]
        public int CourseId { get; set; }
    }
}
