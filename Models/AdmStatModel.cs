namespace Inz_Fn.Models
{
    public class AdmStatModel
    {
        internal double investedSum;
        internal int allStocks;
        internal double averagePPS;

        public List<Stock> Stock { get; set; }
    }
}
