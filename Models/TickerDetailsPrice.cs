namespace Inz_Fn.Models
{
    public class TickerDetailsPrice
    {
        public TickDetails tickDetails {  get; set; }
        public TickerPrevClose tickerPrevClose { get; set; }
        public List<Stock_model> stockModels { get; set; }
        public List<Stock_model> stockModels2 { get; set; }

    }
}
