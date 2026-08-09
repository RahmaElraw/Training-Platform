namespace Training_Platform.ViewModels
{
    public class CategoryWithRelatedVM
    {
        public IEnumerable<Category> Categories { get; set; } = [];

        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }

        public string? Query { get; set; }
    }
}
