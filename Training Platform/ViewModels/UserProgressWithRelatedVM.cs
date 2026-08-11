namespace Training_Platform.ViewModels
{
    public class UserProgressWithRelatedVM
    {
        public IEnumerable<UserProgress> UserProgresses { get; set; }
            = Enumerable.Empty<UserProgress>();

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public string? Query { get; set; }
    }
}
