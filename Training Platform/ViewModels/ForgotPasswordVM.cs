using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class ForgotPasswordVM
    {
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]

        public string Email { get; set; } = string.Empty;
        
    }
}
