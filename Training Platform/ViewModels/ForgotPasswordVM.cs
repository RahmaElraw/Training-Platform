using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class ForgotPasswordVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "FieldRequired")]
        [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
        [Display(Name = "Email")]

        public string Email { get; set; } = string.Empty;
        
    }
}
