namespace Training_Platform.Models
{
    public class UserProgress
    {
        public int Id { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
    }
}
