using System.ComponentModel.DataAnnotations;

namespace Training_Platform.Models
{
    public class Certificate
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string CertificateNumber { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        public string? CertificateUrl { get; set; }

        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}
