namespace PopZebra.Models
{
    public class WorkItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    }
}