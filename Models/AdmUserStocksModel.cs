using System.ComponentModel.DataAnnotations;

namespace Inz_Fn.Models
{
    public class AdmUserStocksModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public AdmStatHitsModel AdmStatHitsModel { get; set; }
        public AdmStatModel AdmStatModel { get; set; }
    }
}
