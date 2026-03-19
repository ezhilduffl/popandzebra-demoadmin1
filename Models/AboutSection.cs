namespace PopZebra.Models
{
    public class AboutSection
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<AboutIcon> Icons { get; set; } = new();
    }
}