using System.ComponentModel.DataAnnotations;

namespace Inz_Fn.Models
{
    public class Stock
    {
        [Key]
        public int Id {  get; set; }
        [Required]
        public string Stock_CIK { get; set; }
        [Required]
        public string User_Id { get; set; }
        [Required]
        public double Price_per_stock { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int Amount { get; set; } 
    }
}
