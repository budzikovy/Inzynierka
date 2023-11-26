using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Inz_Fn.Models;

namespace Inz_Fn.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockController : Controller
    {
        public async Task<IActionResult> Stock()
        {
            List<Stock_model> stockModels = await GetStockData();
            return View(stockModels);
        }


        private async Task<List<Stock_model>> GetStockData()
        {
            string apiKey = "TuP9o6bqsfqxilONFO1cVhApCcvy7wTR";
            string symbol = "AAPL";
            string apiUrl = $"https://api.polygon.io/v2/aggs/ticker/{symbol}/range/1/day/2021-01-01/2021-12-31?apiKey={apiKey}";

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
    }
}
