namespace Training_Platform.Models
{
    public class ApplicationUserOTP
    {
        public int Id { get; set; }
        public int ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public string OTP { get; set; }
        public DateTime CreateAt { get; set; }= DateTime.Now;
        public DateTime ExpireAt { get; set; } = DateTime.Now.AddMinutes(10);
        public bool IsUsed { get; set; }
    }
}
