namespace PopZebra.Models
{
    public class ShopItem
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<ShopItemHistory> History { get; set; } = new();
    }
}