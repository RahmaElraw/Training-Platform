using System.ComponentModel.DataAnnotations;

namespace Training_Platform.ViewModels
{
    public class CategoryVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [MaxLength(400)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
