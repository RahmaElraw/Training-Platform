using Microsoft.AspNetCore.Mvc.Rendering;

namespace Training_Platform.ViewModels
{
    public class EditUserVM
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? ProfileImage { get; set; }

        public bool IsApproved { get; set; }

        public string? Password { get; set; }

        public string? ConfirmPassword { get; set; }
    }


}