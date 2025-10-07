using ClientPortal.Models;  // <-- was Moz.Models
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moz.Services;
using System.Diagnostics;

namespace ClientPortal.Controllers   // <-- was Moz.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IBank_interface Bank_Interface;

        public HomeController(ILogger<HomeController> logger, IBank_interface bank_Interface)
        {
            _logger = logger;
            Bank_Interface = bank_Interface;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Banks()
        {
         ViewBag.banks= await  Bank_Interface.GetBanks();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
