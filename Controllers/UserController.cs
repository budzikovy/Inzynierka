using Inz_Fn.Areas.Identity.Data;
using Inz_Fn.Data;
using Inz_Fn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using ServiceStack;
using System.Runtime.CompilerServices;


namespace Inz_Fn.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<Inz_FnUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private IMemoryCache _memoryCache;
        private const string StockTickersCacheKey = "StockTickersCacheKey";

        public UserController(UserManager<Inz_FnUser> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager, IMemoryCache memoryCache)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
            _memoryCache = memoryCache;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> CurrentStocks()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge(); // Prompt the user to log in if not already
            }
            var allStocks = await GetGroupedDaily();
            List<CurrentStockViewModel> currentUserStocks = new List<CurrentStockViewModel>();
            var userStocks = _context.Stock.Where(s => s.User_Id == currentUser.Id).ToList();
            foreach (var item in userStocks)
            {
                var model = new CurrentStockViewModel
                {
                    Id = item.Id,
                    Stock_CIK = item.Stock_CIK,
                    User_Id = item.User_Id,
                    Price_per_stock = item.Price_per_stock,
                    Date = item.Date,
                    Amount = item.Amount,
                    curretnPrice = allStocks.Where(s => s.T == item.Stock_CIK).ToList().FirstOrDefault().c
                };
                currentUserStocks.Add(model);
            }
            return View(currentUserStocks);
        }
        public async Task<IActionResult> SellStock(Guid id)
        {
            var stock = _context.Stock.Find(id);
            if (stock == null)
            {
                return NotFound();
            }
            TickerPrevClose TickerPrevClose = await GetStockPreviousClose(stock.Stock_CIK);

            // Sprawdź, czy użytkownik jest zalogowany i czy akcja należy do niego
            var currentUser = _userManager.GetUserAsync(User).Result;
            if (currentUser == null || currentUser.Id != stock.User_Id)
            {
                return Challenge();
            }

            // Przygotuj model do przekazania do widoku
            var model = new SellStockViewModel
            {
                StockId = stock.Id,
                StockCIK = stock.Stock_CIK,
                PricePerStock = stock.Price_per_stock,
                Date = stock.Date,
                CurrentPrice = TickerPrevClose.c,
                Amount = stock.Amount
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SellStockPerm(SellStockViewModel model)
        {
            
            var stock = _context.Stock.Find(model.StockId);
            if (stock == null || model.Amount > stock.Amount)
            {
                return View("NotFound");
            }
            else
            {
                stock.Amount -= model.Amount;
                Console.WriteLine(model.CurrentPrice);
                Console.WriteLine(model.Amount);
                Console.WriteLine(stock.Price_per_stock);
                var historyModel = new StockHistory
                {
                    Stock_CIK = stock.Stock_CIK,
                    User_Id = stock.User_Id,
                    Type_of_action = "sale",
                    Price_per_stock_b = stock.Price_per_stock,
                    Price_per_stock_s = model.CurrentPrice,
                    Date = stock.Date,
                    Amount = model.Amount,
                    SellDate = DateTime.Now,
                    Income = Math.Round(model.Amount * model.CurrentPrice - model.Amount * stock.Price_per_stock, 2)
            };
                if (stock.Amount == 0)
                {
                    _context.Stock.Remove(stock);

                }
                else
                {
                    _context.Stock.Update(stock);
                }
                _context.StocksHistory.Add(historyModel);
                await _context.SaveChangesAsync();

                return RedirectToAction("CurrentStocks");

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

        public async Task<IActionResult> StockHistory()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge(); // Prompt the user to log in if not already
            }

            var userStocksB = _context.StocksHistory.Where(s => s.User_Id == currentUser.Id).Where(x=>x.Type_of_action=="purchase").ToList();
            var userStocksS = _context.StocksHistory.Where(s => s.User_Id == currentUser.Id).Where(x=>x.Type_of_action=="sale").ToList();
            var userStocksAll = _context.StocksHistory.Where(s => s.User_Id == currentUser.Id).ToList();
            var activeStocks = _context.Stock.Where(s => s.User_Id == currentUser.Id).ToList();
            var model = new StatiscticStockViewModel
            {
                StockHistB = userStocksB,
                StockHistS = userStocksS,
                All = userStocksAll,
                Active = activeStocks
            };
            return View(model);
        }
        public async Task<IActionResult> Favourite(string? searchString, string? sort, string? sortOrder, int currentPage = 1, int pageSize = 20) {
            var user = await _userManager.GetUserAsync(User);
            List<StockTickers> stockTickers = new List<StockTickers>();
            List<StockTickers> favS = new List<StockTickers>();
            List<FavouriteStocks> favouriteStocks = _context.FavouriteStocks.Where(x => x.User_Id == user.Id).ToList();
            stockTickers = await GetGroupedDaily();
            favS = stockTickers
                .Where(st => favouriteStocks.Any(fs => fs.Stock_CIK == st.T))
                .ToList();
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToUpper();
                stockTickers = stockTickers.Where(s => s.T.Contains(searchString)).ToList();
            }
            if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "Symbol":
                        favS = sortOrder == "asc" ? favS.OrderBy(s => s.T).ToList() : favS.OrderByDescending(s => s.T).ToList();
                        break;
                    case "Cena":
                        favS = sortOrder == "asc" ? favS.OrderBy(s => s.c).ToList() : favS.OrderByDescending(s => s.c).ToList();
                        break;
                    case "Zmiana":
                        favS = sortOrder == "asc" ? favS.OrderBy(s => s.dailyChange).ToList() : favS.OrderByDescending(s => s.dailyChange).ToList();
                        break;
                    case "Transakcje":
                        favS = sortOrder == "asc" ? favS.OrderBy(s => s.n).ToList() : favS.OrderByDescending(s => s.n).ToList();
                        break;
                        // Dodaj przypadki dla innych kolumn
                }
            }
            var count = favS.Count();
            var items = favS.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            var model = new FavouriteStockViewModel
            {
                StockTickers = favS,
                Pagination = new PaginationModel
                {
                    CurrentPage = currentPage,
                    ItemsPerPage = pageSize,
                    TotalItems = count,
                },
                searchStr = searchString,
                sort = sort,
                sortOrder = sortOrder
            };
            return View(model);
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
        [HttpPost]
        public async Task<IActionResult> DeleteFavourite(string Stock_CIK)
        {
            //First Fetch the User you want to Delete
            Console.WriteLine(Stock_CIK);
            Console.WriteLine(Stock_CIK);
            Console.WriteLine(Stock_CIK);


            var fav = _context.FavouriteStocks.FirstOrDefault(x=>x.Stock_CIK== Stock_CIK);
            if (fav!=null) {
                Console.WriteLine("Działa");
                _context.FavouriteStocks.Remove(fav);
                await _context.SaveChangesAsync();
                return RedirectToAction("Favourite");

            }else { return RedirectToAction("Favourite"); };

        }
    }
}
