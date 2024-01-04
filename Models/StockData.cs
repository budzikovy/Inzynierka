using System;
using System.ComponentModel.DataAnnotations;

namespace Inz_Fn.Models
{
    public class StockData
    {
        [Required]
        public string Ticker { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Market { get; set; }

        [Required]
        public string Locale { get; set; }

        [Required]
        public string PrimaryExchange { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public bool Active { get; set; }

        [Required]
        public string CurrencyName { get; set; }

        [Required]
        public string Cik { get; set; }

        [Required]
        public string CompositeFigi { get; set; }

        [Required]
        public string ShareClassFigi { get; set; }

        [Required]
        public DateTime LastUpdatedUtc { get; set; }
    }
}