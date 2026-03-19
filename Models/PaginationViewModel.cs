namespace PopZebra.Models
{
    public class PaginationViewModel
    {
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }

        // Computed — no setter needed, not assigned in initializer
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        // Function to build page URL
        public Func<int, string> Url { get; set; } = _ => "#";
    }
}