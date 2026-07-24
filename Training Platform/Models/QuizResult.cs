using System.ComponentModel.DataAnnotations;

namespace Training_Platform.Models
{
    public class QuizResult
    {
        public int Id { get; set; }
        [Required]
        public int Score { get; set; }
        public bool IsPassed { get; set; }
        [Required]
        public DateTime SubmittedAt { get; set; }

        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
    }
}
