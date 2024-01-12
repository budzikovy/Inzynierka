namespace Inz_Fn.Models
{

    public class StockTickersViewModel
    {

        internal string? searchStr;
        internal string? sort;
        internal string? sortOrder;

        public List<StockTickers> StockTickers { get; set; }
        public List<FavouriteStocks> Fav { get; set; }
        public List<StockTickers> bigIncrease { get; set; }
        public List<StockTickers> bigVol { get; set; }
        public List<StockTickers> bigLose { get; set; }
        public PaginationModel Pagination { get; set; }
        public List<string> StockTickerCIK { get; internal set; }
    }

}
