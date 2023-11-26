using ServiceStack;

namespace Inz_Fn.Models
{
    public class AVConnection
    {
        private readonly string _apiKey;

        public AVConnection(string apiKey)
        {
            this._apiKey = apiKey;
        }

        public List<SecurityData> GetDailyPrices(string symbol)
        {
            const string FUNCTION = "CRYPTO_INTRADAY";
            string connectionString = "https://" + $@"www.alphavantage.co/query?function={FUNCTION}&symbol={symbol}&market=USD&interval=5min&apikey={this._apiKey}&datatype=csv";
            List<SecurityData> prices = connectionString.GetStringFromUrl().FromCsv<List<SecurityData>>();
            return prices;
        }
    }
}
