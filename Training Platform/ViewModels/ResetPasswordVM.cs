
using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class ResetPasswordVM
    {
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare(nameof(Password), ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "'{0}' and '{1}' do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
