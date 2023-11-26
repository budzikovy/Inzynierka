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
        [HttpGet]
        public async Task<ViewResult> Get()
        {
            string stockData = await GetStockData();
            var model = new StockData { Data = stockData };
            return View(model);
        }


        private async Task<string> GetStockData()
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
                return json.ToString();
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }
    }
}
