﻿using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Inz_Fn.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Inz_Fn.Data;
using Microsoft.AspNetCore.Identity;
using Inz_Fn.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace Inz_Fn.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class StockController : Controller
    {


        private readonly UserManager<Inz_FnUser> _userManager;
        private readonly ApplicationDbContext _context;

        public StockController(UserManager<Inz_FnUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }        
        
        [HttpPost("PurchaseStock")]
        public async Task<IActionResult> PurchaseStock([FromForm] StockViewModel formStock)
        {
            string symbol = formStock.Stock_CIK;
            double pricePerStock= formStock.Price_per_stock;
            var model = new StockViewModel
            {
                Stock_CIK = symbol,
                Price_per_stock = pricePerStock
            };
            return View(model);
        }


        [HttpPost("SaveStock")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStock([FromForm] StockViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var stock = new Stock
            {
                Stock_CIK = model.Stock_CIK,
                User_Id = user.Id,
                Price_per_stock = model.Price_per_stock,
                Date = DateTime.Now, // Ustawiamy aktualną datę, zakładając że to jest data transakcji
                Amount = model.Amount
            };

            _context.Stock.Add(stock);
            await _context.SaveChangesAsync();

            return Ok();
        }

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
            TickerPrevClose TickerPrevClose = await GetStockPreviousClose(id);
            if (TickDetails.branding != null) {
                if (TickDetails.branding.logo_url != null)
                {
                    TickDetails.branding.logo_url += "?apiKey=" + apiKey;
                }
                if (TickDetails.branding.icon_url != null)
                {
                    TickDetails.branding.icon_url += "?apiKey=" + apiKey;
                }
            }
            
            TickerDetailsPrice tickerDetailsPrice = new TickerDetailsPrice
            {
                tickDetails=TickDetails,
                tickerPrevClose = TickerPrevClose
            };
            return View(tickerDetailsPrice);
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
        private async Task<TickerPrevClose> GetStockPreviousClose(string Ticker)
        {
            string apiKey = "TuP9o6bqsfqxilONFO1cVhApCcvy7wTR";
            string symbol = Ticker;
            string apiUrl = $"https://api.polygon.io/v2/aggs/ticker/{symbol}/prev?adjusted=true&apiKey={apiKey}";

            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(content);
                JToken results = json["results"];

                List<TickerPrevClose> tickersPrevClose = new List<TickerPrevClose>();
                foreach (JToken result in results)
                {
                    TickerPrevClose tickerPrev= result.ToObject<TickerPrevClose>();
                    tickersPrevClose.Add(tickerPrev);
                }
                TickerPrevClose tickerPrevClose = new TickerPrevClose();
                tickerPrevClose = tickersPrevClose[0];
                return tickerPrevClose;
            }
            else
            {
                throw new Exception($"Error: {response.StatusCode}");
            }
/*
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(content);
                JToken results = json["results"];

                TickerPrevClose tickerPrevClose = new TickerPrevClose();
                tickerPrevClose = results.ToObject<TickerPrevClose>();
                return tickerPrevClose;
            }
            else
            {
                throw new Exception($"Error: {response.StatusCode}");
            }*/

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
