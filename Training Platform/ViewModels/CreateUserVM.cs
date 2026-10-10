using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class CreateUserVM
    {
        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "Selected Role")]
        public string SelectedRole { get; set; } = string.Empty;

        public IEnumerable<SelectListItem>? Roles { get; set; }

        public IFormFile? ProfileImageFile { get; set; }
    }
}

