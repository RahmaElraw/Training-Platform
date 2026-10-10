using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class EditUserVM
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? ProfileImage { get; set; }

        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "'{0}' and '{1}' do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}