using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class RegisterVM
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FName { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Last Name")]
        public string LName { get; set; } = string.Empty;
        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImageFile { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? Address { get; set; }

    }
}
