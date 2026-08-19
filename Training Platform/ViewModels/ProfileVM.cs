using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class ProfileVM
    {
            public string Email { get; set; } = string.Empty;
            public string? ProfileImage { get; set; }

            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; } = string.Empty;

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Address")]
            public string Address { get; set; } = string.Empty;

            [Required]
            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; } = string.Empty;

            public IFormFile? ProfileImageFile { get; set; }
    }
}
