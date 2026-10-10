using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class EnrollmentVM
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; }

        [Required]
        [Display(Name = "User Id")]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Course Id")]
        public int CourseId { get; set; }

        [Display(Name = "Is Completed")]
        public bool IsCompleted { get; set; }
    }
}
