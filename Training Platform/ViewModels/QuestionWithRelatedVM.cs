namespace Training_Platform.ViewModels
{
    public class QuestionWithRelatedVM
    {
        public IEnumerable<Question> Questions { get; set; }
            = Enumerable.Empty<Question>();

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public string? Query { get; set; }
    }
}
