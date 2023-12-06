namespace Inz_Fn.Models
{
    public class AggregatesViewModel
    {
        public string symbol {  get; set; }
        public int multiplier { get; set; }
        public string timespan { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }

    }
}