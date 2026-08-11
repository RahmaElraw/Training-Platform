namespace Training_Platform.ViewModels
{
    public class UserVM
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? ProfileImage { get; set; }

        public bool IsApproved { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Role { get; set; } = string.Empty;
    }
}