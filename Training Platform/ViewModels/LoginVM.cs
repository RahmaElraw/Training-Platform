using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class LoginVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "FieldRequired")]
        [Display(Name = "Email Or Full Name")]

        public string EmailOrUserName { get; set; } = string.Empty;
        [Required(ErrorMessage = "FieldRequired")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }= string.Empty;
        [Display(Name = "RememberMe")]
        public bool RememberMe { get; set; }
    }
}
