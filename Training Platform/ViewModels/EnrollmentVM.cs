using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class EnrollmentVM
    {
        public int Id { get; set; }

        [Required]
        public DateTime EnrollmentDate { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public bool IsCompleted { get; set; }
    }
}
