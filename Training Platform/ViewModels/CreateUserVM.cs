using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Training_Platform.ViewModels
{
    public class CreateUserVM
    {
        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsApproved { get; set; }

        public string SelectedRole { get; set; } = string.Empty;

        public IEnumerable<SelectListItem>? Roles { get; set; }

        public IFormFile? ProfileImageFile { get; set; }
    }
}

