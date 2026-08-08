namespace Training_Platform.ViewModels
{
    public class UserWithRelatedVM
    {
        public IEnumerable<UserWithRoleVM> Users { get; set; }
        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public string? Query { get; set; }
    }
}