using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class CourseVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(400)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Range(1, 1000)]
        [Display(Name = "Duration In Hours")]
        public int DurationInHours { get; set; }

        [Display(Name = "Thumbnail")]
        public string? Thumbnail { get; set; }

        [Required]
        [Display(Name = "Level")]
        public CourseLevel Level { get; set; }

        [Display(Name = "Is Published")]
        public bool IsPublished { get; set; }

        [Required]
        [Display(Name = "Category Id")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Trainer Id")]
        public int TrainerId { get; set; }
    }
}
