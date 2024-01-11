using System.ComponentModel.DataAnnotations;
namespace Inz_Fn.Models
{
    public class StockHistory
    {
        public StockHistory()
        {
            Id = Guid.NewGuid();
        }
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Stock_CIK { get; set; }
        [Required]
        public string User_Id { get; set; }
        [Required]
        public string Type_of_action { get; set; }
        [Required]
        public double Price_per_stock_b { get; set; }
        public double? Price_per_stock_s { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int Amount { get; set; }
        public DateTime? SellDate { get; set; }
        public double? Income { get; set; }
        public string Currency { get; internal set; }
    }
}
