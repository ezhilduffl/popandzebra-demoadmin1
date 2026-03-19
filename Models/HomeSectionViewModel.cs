using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PopZebra.Models
{
    public class HomeSectionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an option for Set Image For.")]
        public bool? SetImageFor { get; set; }

        // ── NEW ──────────────────────────────────────────────
        [MaxLength(500, ErrorMessage = "Link URL cannot exceed 500 characters.")]
        public string? LinkUrl { get; set; }
        // ─────────────────────────────────────────────────────

        public IFormFile? SingleImage { get; set; }
        public IFormFile? MobileImage { get; set; }
        public IFormFile? DesktopImage { get; set; }

        public string? ExistingSingleImagePath { get; set; }
        public string? ExistingMobileImagePath { get; set; }
        public string? ExistingDesktopImagePath { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}