namespace Training_Platform.ViewModels
{
    public class UserWithRoleVM
    {
        public ApplicationUser User { get; set; } = null!;

        public string Role { get; set; } = string.Empty;
    }
}
