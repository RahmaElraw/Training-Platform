using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class CourseVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(400)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int DurationInHours { get; set; }

        public string? Thumbnail { get; set; }

        [Required]
        public CourseLevel Level { get; set; }

        public bool IsPublished { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int TrainerId { get; set; }
    }
}
