using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PopZebra.Models
{
    public class AboutIconViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Icon title is required.")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        public IFormFile IconFile { get; set; }

        [Required(ErrorMessage = "Link URL is required.")]
        [MaxLength(500)]
        public string LinkUrl { get; set; } = string.Empty;

        // Existing path shown on edit
        public string ExistingIconPath { get; set; }

        public int SortOrder { get; set; }
    }
}