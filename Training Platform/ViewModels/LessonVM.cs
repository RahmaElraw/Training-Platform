using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class LessonVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Video Url")]
        public string VideoUrl { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Order Number")]
        public int OrderNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Course Id")]
        public int CourseId { get; set; }
        public ICollection<CourseMaterial> CourseMaterials { get; set; }
            = new List<CourseMaterial>();
    }
}
