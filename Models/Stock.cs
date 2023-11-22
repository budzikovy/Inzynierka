using System.ComponentModel.DataAnnotations;

namespace Inz_Fn.Models
{
    public class Stock
    {
        [Key]
        public int Id {  get; set; }
        [Required]
        public int Stock_Id { get; set; }
        [Required]
        public int User_Id { get; set; }
        [Required]
        public int Price_per_stock { get; set; }
        [Required]
        public int Amount { get; set; } 

    }
}
