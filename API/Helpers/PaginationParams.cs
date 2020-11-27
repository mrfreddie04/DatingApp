namespace API.Helpers
{
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10; //default page size

        public int PageNumber { get; set; } = 1; //default value

        public int PageSize 
        { 
            get => _pageSize; 
            set => _pageSize = (value <= MaxPageSize) ? value : MaxPageSize;
        }        
    }
}