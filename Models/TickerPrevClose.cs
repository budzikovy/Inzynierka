using System.Net;

namespace Inz_Fn.Models
{
    public class TickerPrevClose
    {
        public string T { get; set; } // Ticker symbol
        public long v { get; set; } // Volume
        public double vw { get; set; } // Volume Weighted Average Price
        public double o { get; set; } // Open price
        public double c { get; set; } // Close price
        public double h { get; set; } // High price
        public double l { get; set; } // Low price
        public long t { get; set; } // Timestamp
        public int n { get; set; } // Number of transactions
    }
}
