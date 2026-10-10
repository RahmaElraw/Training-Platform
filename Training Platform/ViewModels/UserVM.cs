using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class UserVM
    {
        public int Id { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? ProfileImage { get; set; }

        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Role { get; set; } = string.Empty;
    }
}