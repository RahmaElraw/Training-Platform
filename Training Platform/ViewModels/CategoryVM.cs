using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class CategoryVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(400)]
        public string? Description { get; set; }
    }
}
