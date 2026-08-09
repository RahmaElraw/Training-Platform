namespace Training_Platform.ViewModels
{
    public class ReviewWithRelatedVM
    {
        public IEnumerable<Review> Reviews { get; set; } = [];

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }

        public string? Query { get; set; }
    }
}
