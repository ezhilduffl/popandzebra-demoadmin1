using System.ComponentModel.DataAnnotations;

namespace PopZebra.Models
{
    public class AboutSectionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

        // Always exactly 5 icon slots
        public List<AboutIconViewModel> Icons { get; set; } = new()
        {
            new() { SortOrder = 1 },
            new() { SortOrder = 2 },
            new() { SortOrder = 3 },
            new() { SortOrder = 4 },
            new() { SortOrder = 5 }
        };
    }
}