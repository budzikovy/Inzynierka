using Inz_Fn.Areas.Identity.Data;
using Inz_Fn.Data;
using Inz_Fn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ServiceStack.Script.Lisp;

namespace Inz_Fn.Controllers
{


    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<Inz_FnUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<Inz_FnUser> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }
        public ActionResult Index()
        {
            return View();
        }

        public IActionResult Users()
        {
            var users = _userManager.Users;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string UserId)
        {
            //First Fetch the User Details by UserId
            var user = await _userManager.FindByIdAsync(UserId);
            //Check if User Exists in the Database
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }
            // GetClaimsAsync retunrs the list of user Claims
            var userClaims = await _userManager.GetClaimsAsync(user);
            // GetRolesAsync returns the list of user Roles
            var userRoles = await _userManager.GetRolesAsync(user);
            //Store all the information in the EditUserViewModel instance
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = userRoles
            };
            //Pass the Model to the View
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                //Populate the user instance with the data from EditUserViewModel
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.EmailConfirmed = model.EmailConfirmed;
                //UpdateAsync Method will update the user data in the AspNetUsers Identity table
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    //Once user data updated redirect to the ListUsers view
                    return RedirectToAction("Users");
                }
                else
                {
                    //In case any error, stay in the same view and show the model validation error
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                return View(model);
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string UserId)
        {
            //First Fetch the User you want to Delete
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                // Handle the case where the user wasn't found
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }
            else
            {
                //Delete the User Using DeleteAsync Method of UserManager Service
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    // Handle a successful delete
                    return RedirectToAction("Users");
                }
                else
                {
                    // Handle failure
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                return View("Users");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string UserId)
        {
            //First Fetch the User Information from the Identity database by user Id
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                //handle if User Not Found in the Database
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }
            //Store the UserId in the ViewBag which is required while updating the Data
            //Store the UserName in the ViewBag which is used for displaying purpose
            ViewBag.UserId = UserId;
            ViewBag.UserName = user.UserName;
            //Create a List to Hold all the Roles Information
            var model = new List<UserRolesViewModel>();
            //Loop Through Each role and populate the model 
            foreach (var role in await _roleManager.Roles.ToListAsync())
            {
                var userRolesViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                //Check if the Role is already assigned to this user
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.IsSelected = true;
                }
                else
                {
                    userRolesViewModel.IsSelected = false;
                }
                //Add the userRolesViewModel to the model
                model.Add(userRolesViewModel);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }

            //fetch the list of roles the specified user belongs to
            var roles = await _userManager.GetRolesAsync(user);

            //Then remove all the assigned roles for this user
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }

            List<string> RolesToBeAssigned = model.Where(x => x.IsSelected).Select(y => y.RoleName).ToList();

            //If At least 1 Role is assigned, Any Method will return true
            if (RolesToBeAssigned.Any())
            {
                //add a user to multiple roles simultaneously

                result = await _userManager.AddToRolesAsync(user, RolesToBeAssigned);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot Add Selected Roles to User");
                    return View(model);
                }
            }

            return RedirectToAction("EditUser", new { UserId = UserId });
        }
        public IActionResult ActiveStocks() 
        { 
            var activeStocks = _context.Stock.ToList();
            var sum = 0.00;
            var allStocks = _context.Stock.Sum(x => x.Amount);
            foreach (var stock in activeStocks) 
            {
                sum += Math.Round(stock.Price_per_stock * stock.Amount, 2);
            }
            var model = new AdmStatModel
            {   
                averagePPS = sum/allStocks,
                allStocks = allStocks,
                investedSum = sum,
                Stock = activeStocks
            };
            return View(model);
        }
        public IActionResult HistoryStocks() 
        { 
            var activeStocks = _context.StocksHistory.ToList();
            var sum = 0.00;
            var allStocks = _context.StocksHistory.Sum(x => x.Amount);/*
            foreach (var stock in activeStocks) 
            {
                sum += Math.Round(stock.Price_per_stock * stock.Amount, 2);
            }
            var model = new AdmStatModel
            {   
                averagePPS = sum/allStocks,
                allStocks = allStocks,
                investedSum = sum,
                Stock = activeStocks
            };*/
            return View();
        }

        public async Task<IActionResult> UserStocks(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }
            var userStocksB = _context.StocksHistory.Where(x=>x.User_Id==user.Id).Where(x => x.Type_of_action == "purchase").ToList();
            var userStocksS = _context.StocksHistory.Where(x => x.User_Id == user.Id).Where(x => x.Type_of_action == "sale").ToList();
            var userStocksAll = _context.StocksHistory.Where(x => x.User_Id == user.Id).ToList();
            var activeStocks = _context.Stock.Where(x => x.User_Id == user.Id).ToList();
            var Stat = new StatiscticStockViewModel
            {
                StockHistB = userStocksB,
                StockHistS = userStocksS,
                All = userStocksAll,
                Active = activeStocks
            };
            var model = new AdmUserStocksModel
            {
                Id = user.Id,
                Email = user.Email,
                StatiscticStock = Stat
            };
            /*var sum = 0.00;
            foreach (var stock in activeStocks)
            {
                sum += Math.Round(stock.Price_per_stock * stock.Amount, 2);
            }

            var admStock = new AdmStatModel
            {
                averagePPS = sum / allStocks,
                allStocks = allStocks,
                investedSum = sum,
                Stock = activeStocks
            };
            var allStocksH = _context.StocksHistory.Where(x => x.User_Id == UserId).Sum(x => x.Amount);
            var activeStocksH = _context.StocksHistory.Where(x => x.User_Id == UserId).ToList();
            var sumH = 0.00;
            foreach (var stock in activeStocks)
            {
                sumH += Math.Round(stock.Price_per_stock * stock.Amount, 2);
            }
            var admStockHis = new AdmStatHitsModel 
            {
                averagePPS = sumH / allStocksH,
                allStocks = allStocksH,
                investedSum = sumH,
                StockHist = activeStocksH
            };
            var model = new AdmUserStocksModel 
            { 
                Id = user.Id,
                Email = user.Email,
                AdmStatHitsModel = admStockHis,
                AdmStatModel = admStock

            };*/
            return View(model);
        }
        public IActionResult Statistics()
        {

            var userStocksB = _context.StocksHistory.Where(x => x.Type_of_action == "purchase").ToList();
            var userStocksS = _context.StocksHistory.Where(x => x.Type_of_action == "sale").ToList();
            var userStocksAll = _context.StocksHistory.ToList();
            var activeStocks = _context.Stock.ToList();
            var Stat = new StatiscticStockViewModel
            {
                StockHistB = userStocksB,
                StockHistS = userStocksS,
                All = userStocksAll,
                Active = activeStocks
            };
            var model = new AdmUserStocksModel
            {
                StatiscticStock = Stat
            };
            /*var sum = 0.00;
            foreach (var stock in activeStocks)
            {
                sum += Math.Round(stock.Price_per_stock * stock.Amount, 2);
            }

            var admStock = new AdmStatModel
            {
                averagePPS = sum / allStocks,
                allStocks = allStocks,
                investedSum = sum,
                Stock = activeStocks
            };
            var allStocksH = _context.StocksHistory.Where(x => x.User_Id == UserId).Sum(x => x.Amount);
            var activeStocksH = _context.StocksHistory.Where(x => x.User_Id == UserId).ToList();
            var sumH = 0.00;
            foreach (var stock in activeStocks)
            {
                sumH += Math.Round(stock.Price_per_stock * stock.Amount, 2);
            }
            var admStockHis = new AdmStatHitsModel 
            {
                averagePPS = sumH / allStocksH,
                allStocks = allStocksH,
                investedSum = sumH,
                StockHist = activeStocksH
            };
            var model = new AdmUserStocksModel 
            { 
                Id = user.Id,
                Email = user.Email,
                AdmStatHitsModel = admStockHis,
                AdmStatModel = admStock

            };*/
            return View(model);
        }
    }
}
