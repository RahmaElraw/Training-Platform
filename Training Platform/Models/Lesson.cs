using System.ComponentModel.DataAnnotations;

namespace Training_Platform.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        [Required]
        public string VideoUrl { get; set; }
        [Required]
        public int OrderNumber { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public ICollection<CourseMaterial> CourseMaterials { get; set; } = new List<CourseMaterial>();
        public ICollection<UserProgress> UserProgresses { get; set; } = new List<UserProgress>();
    }
}
