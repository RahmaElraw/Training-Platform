using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;

namespace Training_Platform.Models
{
    public enum CourseLevel
    {
        Beginner,
        Intermediate,
        Advanced
    }

    public class Course
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MaxLength(400)]
        public string Description { get; set; }
        public int DurationInHours { get; set; }
        public string? Thumbnail { get; set; }
        [Required]
        public CourseLevel Level { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int TrainerId { get; set; }
        public ApplicationUser Trainer { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
