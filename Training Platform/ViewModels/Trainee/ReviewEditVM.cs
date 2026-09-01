namespace Training_Platform.ViewModels.Trainee
{
    public class ReviewEditVM
    {
        public int Id { get; set; }

        public string? CourseTitle { get; set; }

        [Required(ErrorMessage = "Please select a rating.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }
    }
}
