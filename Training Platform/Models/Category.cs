using System.ComponentModel.DataAnnotations;

namespace Training_Platform.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(400)]
        public string? Description { get; set; }

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
