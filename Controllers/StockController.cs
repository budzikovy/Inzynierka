using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Inz_Fn.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;

namespace Inz_Fn.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StockController : Controller
    {
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            // Użyj danych z modelu do pobrania danych giełdowych
            List<StockTickers> stockTickers = new List<StockTickers>();
            stockTickers = await GetGroupedDaily();
            return View("Index", stockTickers);
        }

        [HttpGet("Aggregate")]
        public async Task<IActionResult> Aggregate()
        {
            // Pobierz dane z API
            List<string> symbols =await GetStockTickers();
            List<string> timeUnits = new List<string>
            {
                "second",
                "minute",
                "hour",
                "day",
                "week",
                "month",
                "quarter",
                "year"
            };

            // Przekazanie danych do widoku
            ViewBag.Symbols = new SelectList(symbols);
            ViewBag.TimeUnits = new SelectList(timeUnits);

            return View();
        }
        [HttpGet("TickerDetails")]
        public async Task<IActionResult> TickerDetails(string id)
        {
            string apiKey = "TuP9o6bqsfqxilONFO1cVhApCcvy7wTR";
            TickDetails TickDetails = await GetTickerDetails(id);
            TickDetails.branding.logo_url+= "?apiKey="+apiKey;
            TickDetails.branding.icon_url += "?apiKey=" + apiKey;
            return View(TickDetails);
        }

        private async Task<TickDetails> GetTickerDetails(string tck)
        {
            string apiKey = "TuP9o6bqsfqxilONFO1cVhApCcvy7wTR";
            string ticker = tck;
            string apiUrl = $"https://api.polygon.io/v3/reference/tickers/{ticker}?apiKey={apiKey}";

            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(content);
                JToken results = json["results"];

                TickDetails TickDetails = new TickDetails();
                TickDetails = results.ToObject<TickDetails>();
                return TickDetails;
            }
            else
            {
                throw new Exception($"Error: {response.StatusCode}");
            }
        }

        [HttpPost("Stocks")]
        public async Task<IActionResult> Stocks([FromForm] AggregatesViewModel model)
        {
            // Użyj danych z modelu do pobrania danych giełdowych
            List<Stock_model> stockModels = await GetStockData(model);
            return View("Stocks", stockModels);
        }


        private async Task<List<Stock_model>> GetStockData(AggregatesViewModel model)
        {
            string apiKey = "TuP9o6bqsfqxilONFO1cVhApCcvy7wTR";
            string symbol = model.symbol;
            string multiplier = model.multiplier.ToString();
            string timespan = model.timespan;
            string from = model.from.ToString("yyyy-MM-dd");
            string to = model.to.ToString("yyyy-MM-dd");
            string apiUrl = $"https://api.polygon.io/v2/aggs/ticker/{symbol}/range/{multiplier}/{timespan}/{from}/{to}?apiKey={apiKey}";

            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(content);
                JToken results = json["results"];

                List<Stock_model> stockModels = new List<Stock_model>();
                foreach (JToken result in results)
                {
                    Stock_model stockModel = result.ToObject<Stock_model>();
                    stockModels.Add(stockModel);
                }

                return stockModels;
            }
            else
            {
                throw new Exception($"Error: {response.StatusCode}");
            }
        } 
        private async Task<List<StockTickers>> GetGroupedDaily()
        {
            string apiKey = "TuP9o6bqsfqxilONFO1cVhApCcvy7wTR";
            string apiUrl = $"https://api.polygon.io/v2/aggs/grouped/locale/us/market/stocks/2023-01-09?adjusted=true&apiKey={apiKey}";

            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(content);
                JToken results = json["results"];

                List<StockTickers> stockTickers = new List<StockTickers>();
                foreach (JToken result in results)
                {
                    StockTickers stockTicker = result.ToObject<StockTickers>();
                    stockTickers.Add(stockTicker);
                }
                return stockTickers;
            }
            else
            {
                throw new Exception($"Error: {response.StatusCode}");
            }
        }
        private async Task<List<string>> GetStockTickers() {
            List<StockTickers> stockTickers= new List<StockTickers>();
            stockTickers = await GetGroupedDaily();
            List<string> symbols = new List<string>();
            foreach (StockTickers result in stockTickers)
            {
                symbols.Add(result.T);
            }
            symbols.Sort(); // Sortowanie tickerów alfabetycznie

            return symbols;
        }
    }
}
