using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PopZebra.Models
{
    public class WorkItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Link URL is required.")]
        [MaxLength(500, ErrorMessage = "Link URL cannot exceed 500 characters.")]
        public string LinkUrl { get; set; } = string.Empty;

        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Please select a display order.")]
        [Range(1, 10, ErrorMessage = "Display order must be between 1 and 10.")]
        public int? DisplayOrder { get; set; }

        public string? ExistingImagePath { get; set; }

        public List<int> AvailableOrders { get; set; } = new();

        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}