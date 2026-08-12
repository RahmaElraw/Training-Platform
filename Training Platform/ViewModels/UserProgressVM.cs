using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class UserProgressVM
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;

        public int TotalLessons { get; set; }

        public int CompletedLessons { get; set; }

        public int ProgressPercentage { get; set; }
    }
}
