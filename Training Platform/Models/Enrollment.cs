using System.ComponentModel.DataAnnotations;

namespace Training_Platform.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        [Required]
        public DateTime EnrollmentDate { get; set; }

        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
        public bool IsCompleted { get; set; }
    }
}
