namespace Inz_Fn.Models
{
    public class SellStockViewModel
    {
        public Guid StockId { get; set; }
        public string StockCIK { get; set; }
        public double PricePerStock { get; set; }
        public double CurrentPrice { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }
    }
}
