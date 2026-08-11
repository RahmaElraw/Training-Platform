using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class LessonVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public string VideoUrl { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int OrderNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CourseId { get; set; }
        public ICollection<CourseMaterial> CourseMaterials { get; set; }
            = new List<CourseMaterial>();
    }
}
