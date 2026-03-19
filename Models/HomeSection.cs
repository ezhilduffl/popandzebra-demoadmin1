using System.ComponentModel.DataAnnotations;

namespace PopZebra.Models
{
    public class HomeSection
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        public bool SetImageFor { get; set; }

        public string? SingleImagePath { get; set; }
        public string? MobileImagePath { get; set; }
        public string? DesktopImagePath { get; set; }

        // ── NEW ──────────────────────────────────────────────
        [MaxLength(500)]
        public string? LinkUrl { get; set; }
        // ─────────────────────────────────────────────────────

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    }
}