namespace API.Helpers
{
    public class PaginationHeader
    {
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }

        public PaginationHeader(int itemsPerPage, int currentPage, int totalPages, int totalItems)
        {
            ItemsPerPage = itemsPerPage;
            CurrentPage = currentPage;
            TotalPages = totalPages;
            TotalItems = totalItems;
        }        
    }
}