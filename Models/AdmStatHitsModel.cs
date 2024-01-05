namespace Inz_Fn.Models
{
    public class AdmStatHitsModel
    {
        internal double investedSum;
        internal int allStocks;
        internal double averagePPS;

        public List<StockHistory> StockHist { get; set; }
    }
}
