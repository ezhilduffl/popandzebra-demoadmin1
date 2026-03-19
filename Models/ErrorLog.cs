namespace PopZebra.Models
{
    public class ErrorLog
    {
        public int Id { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string PageName { get; set; }
        public string ErrorLine { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }
}