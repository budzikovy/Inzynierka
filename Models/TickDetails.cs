using System.Net;
 
namespace Inz_Fn.Models
{
    public class TickDetails
    {
        public bool active { get; set; }
        public Address address { get; set; }
        public Branding branding { get; set; }
        public string cik { get; set; }
        public string composite_figi { get; set; }
        public string currency_name { get; set; }
        public string delisted_utc { get; set; }
        public string description { get; set; }
        public string homepage_url { get; set; }
        public string list_date { get; set; }
        public string locale { get; set; }
        public string market { get; set; }
        public long market_cap { get; set; }
        public string name { get; set; }
        public string phone_number { get; set; }
        public string primary_exchange { get; set; }
        public int round_lot { get; set; }
        public string share_class_figi { get; set; }
        public long share_class_shares_outstanding { get; set; }
        public string sic_code { get; set; }
        public string sic_description { get; set; }
        public string ticker { get; set; }
        public string ticker_root { get; set; }
        public string ticker_suffix { get; set; }
        public int total_employees { get; set; }
        public string type { get; set; }
        public long weighted_shares_outstanding { get; set; }
    }
}
