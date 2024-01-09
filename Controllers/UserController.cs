using Inz_Fn.Areas.Identity.Data;
using Inz_Fn.Data;
using Inz_Fn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ServiceStack;

namespace Inz_Fn.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<Inz_FnUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<Inz_FnUser> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
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

            var userStocks = _context.Stock.Where(s => s.User_Id == currentUser.Id).ToList();
            return View(userStocks);
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
    }
}
