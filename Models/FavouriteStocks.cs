using System.ComponentModel.DataAnnotations;

namespace Inz_Fn.Models
{
    public class FavouriteStocks
    {
        public FavouriteStocks()
        {
            Id = Guid.NewGuid();
        }
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Stock_CIK { get; set; }
        [Required]
        public string User_Id { get; set; }
    }
}
