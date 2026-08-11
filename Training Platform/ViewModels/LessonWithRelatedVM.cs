namespace Training_Platform.ViewModels
{
    public class LessonWithRelatedVM
    {
        public IEnumerable<Lesson> Lessons { get; set; }
            = Enumerable.Empty<Lesson>();

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public string? Query { get; set; }
    }
}