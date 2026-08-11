namespace Training_Platform.ViewModels
{
    public class QuizWithRelatedVM
    {
        public IEnumerable<Quiz> Quizzes { get; set; }
            = Enumerable.Empty<Quiz>();

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public string? Query { get; set; }
    }
}
