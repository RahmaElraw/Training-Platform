namespace Training_Platform.ViewModels
{
    public class ChangePasswordVM
    {
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare(nameof(NewPassword), ErrorMessage = "New password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
