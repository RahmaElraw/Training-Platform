using System.ComponentModel.DataAnnotations;

namespace Training_Platform.Models
{
   
        public class Review
        {
            public int Id { get; set; }
            [Required]
            [Range(1, 5)]
            public int Rating { get; set; }
            [MaxLength(1000)]
            public string? Comment { get; set; }
           [Required]
            public DateTime CreatedAt { get; set; }

            public int UserId { get; set; }
            public ApplicationUser User { get; set; }

            public int CourseId { get; set; }
            public Course Course { get; set; }
        }
    
}
