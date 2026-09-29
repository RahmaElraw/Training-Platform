namespace Training_Platform.ViewModels.Trainer
{
    public class CourseIndexVM
    {
        public IEnumerable<Course> Courses { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public string? Query { get; set; }
    }
}
