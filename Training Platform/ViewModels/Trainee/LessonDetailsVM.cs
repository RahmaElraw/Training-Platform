namespace Training_Platform.ViewModels.Trainee
{
    public class LessonDetailsVM
    {
        public Lesson Lesson { get; set; } = null!;

        public Course Course { get; set; } = null!;

        public bool IsCompleted { get; set; }

        public int? PreviousLessonId { get; set; }

        public int? NextLessonId { get; set; }
    }
}
