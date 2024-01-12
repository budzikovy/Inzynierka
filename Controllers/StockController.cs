using Microsoft.AspNetCore.Mvc;
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
using Microsoft.Extensions.Caching.Memory;

namespace Inz_Fn.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class StockController : Controller
    {


        private readonly UserManager<Inz_FnUser> _userManager;
        private readonly ApplicationDbContext _context;
        private IMemoryCache _memoryCache;
        private const string StockTickersCacheKey = "StockTickersCacheKey";


        public StockController(UserManager<Inz_FnUser> userManager, ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _userManager = userManager;
            _context = context;
            _memoryCache = memoryCache;
        }

        [HttpPost("PurchaseStock")]
        public async Task<IActionResult> PurchaseStock([FromForm] StockViewModel formStock)
        {
            string apiKey = "TuP9o6bqsfqxilONFO1cVhApCcvy7wTR";
            TickDetails TickDetails = await GetTickerDetails(formStock.Stock_CIK);
            TickerPrevClose TickerPrevClose = await GetStockPreviousClose(formStock.Stock_CIK);
            var model = new AggregatesViewModel
            {
                symbol = formStock.Stock_CIK,
                multiplier = 1,
                timespan = "day",
                from = DateTime.Now.AddDays(-365),
                to = DateTime.Now

            };
            var model2 = new AggregatesViewModel
            {
                symbol = formStock.Stock_CIK,
                multiplier = 1,
                timespan = "minute",
                from = DateTime.Now.AddDays(-7),
                to = DateTime.Now

            };
            List<Stock_model> StockModels = await GetStockData(model);
            List<Stock_model> StockModels2 = await GetStockData(model2);
            if (TickDetails.branding != null)
            {
                if (TickDetails.branding.logo_url != null)
                {
                    TickDetails.branding.logo_url += "?apiKey=" + apiKey;
                }
                if (TickDetails.branding.icon_url != null)
                {
                    TickDetails.branding.icon_url += "?apiKey=" + apiKey;
                }
            }

            
            string symbol = formStock.Stock_CIK;
            double pricePerStock = formStock.Price_per_stock;
            string currency = formStock.Currency;
            var StockViewModelD = new StockBuyModel
            {
                Stock_CIK = symbol,
                Price_per_stock = pricePerStock,
                Currency = currency,
                stockModels = StockModels,
                stockModels2 = StockModels2
            };
            return View(StockViewModelD);
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
                Currency = model.Currency,
                Date = DateTime.Now, // Ustawiamy aktualną datę, zakładając że to jest data transakcji
                Amount = model.Amount
            };
            var stockHist = new StockHistory
            {
                Stock_CIK = model.Stock_CIK,
                User_Id = user.Id,
                Type_of_action = "purchase",
                Price_per_stock_b = model.Price_per_stock,
                Currency = model.Currency,
                Date = DateTime.Now, // Ustawiamy aktualną datę, zakładając że to jest data transakcji
                Amount = model.Amount
            };

            _context.Stock.Add(stock);
            _context.StocksHistory.Add(stockHist);
            await _context.SaveChangesAsync();

            return RedirectToAction("CurrentStocks", "User");
        }
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? searchString, string? sort, string? sortOrder, int currentPage = 1, int pageSize = 20)
        {
            var user = await _userManager.GetUserAsync(User);

            List<StockTickers> stockTickers = new List<StockTickers>();
            List<StockTickers> stockInc = new List<StockTickers>();
            List<StockTickers> stockVol = new List<StockTickers>();
            List<StockTickers> stockLos = new List<StockTickers>();
            stockTickers = await GetGroupedDaily();
            List<FavouriteStocks> favouriteStocks = _context.FavouriteStocks.Where(x => x.User_Id == user.Id).ToList();
            stockInc = stockTickers.OrderByDescending(s => s.dailyChange).Take(3).ToList();
            stockVol = stockTickers.OrderByDescending(s => s.v).Take(3).ToList();
            stockLos = stockTickers.OrderBy(s => s.dailyChange).Take(3).ToList();

            // Add sorting logic here

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToUpper();
                stockTickers = stockTickers.Where(s => s.T.Contains(searchString)).ToList();
            }
            List<string> stockstr = new List<string>();
            foreach (var stock in stockTickers)
            {
                if (favouriteStocks.Any(fs => fs.Stock_CIK == stock.T))
                {
                    stock.isFavourtie = true;
                }
                else {
                    stock.isFavourtie = false;
                }
                stockstr.Add(stock.T);
            }
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "Symbol":
                        stockTickers = sortOrder == "asc" ? stockTickers.OrderBy(s => s.T).ToList() : stockTickers.OrderByDescending(s => s.T).ToList();
                        break;
                    case "Cena":
                        stockTickers = sortOrder == "asc" ? stockTickers.OrderBy(s => s.c).ToList() : stockTickers.OrderByDescending(s => s.c).ToList();
                        break;
                    case "Zmiana":
                        stockTickers = sortOrder == "asc" ? stockTickers.OrderBy(s => s.dailyChange).ToList() : stockTickers.OrderByDescending(s => s.dailyChange).ToList();
                        break;
                    case "Transakcje":
                        stockTickers = sortOrder == "asc" ? stockTickers.OrderBy(s => s.n).ToList() : stockTickers.OrderByDescending(s => s.n).ToList();
                        break;
                        // Dodaj przypadki dla innych kolumn
                }
            }
            
            var count = stockTickers.Count();
            var items = stockTickers.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            var model = new StockTickersViewModel
            {
                StockTickers = items,
                StockTickerCIK = stockstr,
                Pagination = new PaginationModel
                {
                    CurrentPage = currentPage,
                    ItemsPerPage = pageSize,
                    TotalItems = count,
                },
                searchStr = searchString,
                sort = sort,
                sortOrder = sortOrder,
                Fav = favouriteStocks,
                bigIncrease = stockInc,
                bigLose = stockLos,
                bigVol = stockVol

            };
            return View("Index", model);
        }

        [HttpGet("Aggregate")]
        public async Task<IActionResult> Aggregate()
        {
            // Pobierz dane z API
            List<string> symbols = await GetStockTickers();
            List<string> timeUnits = new List<string>
            {
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
            var model = new AggregatesViewModel {
                symbol = id,
                multiplier =1,
                timespan = "day",
                from = DateTime.Now.AddDays(-365),
                to = DateTime.Now

            };
            var model2 = new AggregatesViewModel {
                symbol = id,
                multiplier =1,
                timespan = "minute",
                from = DateTime.Now.AddDays(-7),
                to = DateTime.Now

            };
            List<Stock_model> StockModels = await GetStockData(model);
            List<Stock_model> StockModels2 = await GetStockData(model2);
            if (TickDetails.branding != null)
            {
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
                tickDetails = TickDetails,
                tickerPrevClose = TickerPrevClose,
                stockModels = StockModels,
                stockModels2 = StockModels2
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
            Console.WriteLine(apiUrl);
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
                    TickerPrevClose tickerPrev = result.ToObject<TickerPrevClose>();
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

        }
        private async Task<List<StockTickers>> GetGroupedDaily()
        {
            if (_memoryCache.TryGetValue(StockTickersCacheKey, out List<StockTickers> cachedStockTickers))
            {
                return cachedStockTickers;
            }
            else
            {
                // Dane nie są dostępne w pamięci podręcznej, wykonaj żądanie do API
                string apiKey = "TuP9o6bqsfqxilONFO1cVhApCcvy7wTR";
                DateTime today = DateTime.Now;

                // Sprawdź dzień tygodnia i przypisz odpowiednią wartość
                int value;
                if (today.DayOfWeek == DayOfWeek.Saturday)
                {
                    value = 2;
                }
                else if (today.DayOfWeek == DayOfWeek.Sunday || today.DayOfWeek == DayOfWeek.Monday)
                {
                    value = 3;
                }
                else
                {
                    value = 1;
                    if (today.Hour < 10)
                    {
                            value += 1;
                    }
                }
                
                Console.WriteLine(today.Hour);
                string date = DateTime.Now.AddDays(-value).ToString("yyyy-MM-dd");
                string apiUrl = $"https://api.polygon.io/v2/aggs/grouped/locale/us/market/stocks/{date}?adjusted=true&apiKey={apiKey}";

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
                    stockTickers = stockTickers.OrderBy(x => x.T).ToList();


                    // Zapisz dane w pamięci podręcznej
                    _memoryCache.Set(StockTickersCacheKey, stockTickers, TimeSpan.FromMinutes(30)); // Dane będą przechowywane przez 30 minut

                    return stockTickers;
                }
                else
                {
                    throw new Exception($"Error: {response.StatusCode}");
                }
            }
        }
        private async Task<List<string>> GetStockTickers()
        {
            List<StockTickers> stockTickers = new List<StockTickers>();
            stockTickers = await GetGroupedDaily();
            List<string> symbols = new List<string>();
            foreach (StockTickers result in stockTickers)
            {
                symbols.Add(result.T);
            }
            symbols.Sort(); // Sortowanie tickerów alfabetycznie

            return symbols;
        }
        [HttpGet]
        public async Task<IActionResult> AddToFavorites(string? stock_CIK)
        {
            if (!string.IsNullOrEmpty(stock_CIK)) {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }
                FavouriteStocks favouriteStock = new FavouriteStocks
                {
                    Stock_CIK = stock_CIK,
                    User_Id = user.Id
                };

                // Add the new object to the database
                _context.FavouriteStocks.Add(favouriteStock);
                await _context.SaveChangesAsync();
            }
            // Redirect the user back to the index page
            return RedirectToAction(nameof(Index));
        }
    }
}