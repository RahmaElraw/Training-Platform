using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class UserProgressVM
    {
        public int Id { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int LessonId { get; set; }
    }
}
