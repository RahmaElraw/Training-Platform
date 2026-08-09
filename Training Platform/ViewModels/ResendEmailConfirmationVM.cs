using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class ResendEmailConfirmationVM
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Email Or Username")]

        public string EmailOrUserName { get; set; } = string.Empty;
        
    }
}
