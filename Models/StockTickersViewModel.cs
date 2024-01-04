namespace Inz_Fn.Models
{
    public class StockTickersViewModel
    {
        public List<StockTickers> StockTickers { get; set; }
        public PaginationModel Pagination { get; set; }
        public string SortOrder { get; set; }
    }
}
