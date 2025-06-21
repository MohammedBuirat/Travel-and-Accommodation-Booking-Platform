namespace Travel_and_Accommodation_API.Helpers
{
    public class Paging
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int MaxPageSize { get; set; } = 100;
        public int DefaultPageSize { get; set; } = 10;

        public Paging(int? pageSize, int? pageNumber)
        {
            pageSize ??= DefaultPageSize;
            pageNumber ??= 1;
            pageSize = Math.Min((int)pageSize, MaxPageSize);
            PageSize = (int)pageSize;
            PageNumber = (int)pageNumber;
            if(PageNumber < 0)
            {
                PageNumber = 1;
            }
            if (PageSize <= 0)
            {
                PageSize = DefaultPageSize;
            }
        }
    }
}
