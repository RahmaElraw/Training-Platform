using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;

namespace Training_Platform.Models
{
    public class ApplicationUser : IdentityUser<int>
    {

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? ProfileImage { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
        public ICollection<Course> CoursesCreated { get; set; } = new List<Course>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<UserProgress> UserProgresses { get; set; } = new List<UserProgress>();
        public ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
