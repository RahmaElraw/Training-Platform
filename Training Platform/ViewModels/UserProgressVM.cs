using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class UserProgressVM
    {
        [Display(Name = "User Id")]
        public int UserId { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Course Id")]
        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;

        public int TotalLessons { get; set; }

        public int CompletedLessons { get; set; }

        public int ProgressPercentage { get; set; }
    }
}
