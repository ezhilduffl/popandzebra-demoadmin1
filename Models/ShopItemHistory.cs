namespace PopZebra.Models
{
    public class ShopItemHistory
    {
        public int Id { get; set; }
        public int ShopItemId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public DateTime SavedOn { get; set; } = DateTime.UtcNow;

        // Navigation
        public ShopItem ShopItem { get; set; }
    }
}