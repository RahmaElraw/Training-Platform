using System.ComponentModel.DataAnnotations;

namespace Training_Platform.Models
{
    public class CourseMaterial
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string? Title { get; set; }
        [Required]
        public string Url { get; set; }

        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
    }
}
