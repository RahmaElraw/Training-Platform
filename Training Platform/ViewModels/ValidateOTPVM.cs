using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class ValidateOTPVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "FieldRequired")]
        [Display(Name = "OTP Number")]

        public string OTP { get; set; } = string.Empty;

    }
}
