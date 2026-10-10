using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class ReviewVM
    {
        public int Id { get; set; }

        [Display(Name = "Rating")]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Display(Name = "Comment")]
        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        [Display(Name = "Course Id")]
        public int CourseId { get; set; }

        [Display(Name = "User Id")]
        public string UserId { get; set; } = string.Empty;

        public string CourseTitle { get; set; } = string.Empty;

        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;
    }
}

