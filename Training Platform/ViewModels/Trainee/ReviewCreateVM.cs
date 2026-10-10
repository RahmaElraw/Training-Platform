namespace Training_Platform.ViewModels.Trainee
{
    public class ReviewCreateVM
    {
        public int CourseId { get; set; }

        public string? CourseTitle { get; set; } 

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}
