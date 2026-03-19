namespace PopZebra.Models
{
    public class AboutIcon
    {
        public int Id { get; set; }
        public int AboutSectionId { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;

        // Navigation
        public AboutSection AboutSection { get; set; }
    }
}