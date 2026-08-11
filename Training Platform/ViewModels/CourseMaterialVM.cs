using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class CourseMaterialVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Url { get; set; } = string.Empty;

        [Required]
        public int LessonId { get; set; }
    }
}
