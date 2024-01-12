namespace Inz_Fn.Models
{
    public class FavouriteStockViewModel
    {
        internal string? searchStr;
        internal string? sort;
        internal string? sortOrder;
        public List<StockTickers> StockTickers { get; set; }
        public PaginationModel Pagination { get; set; }

    }
}
