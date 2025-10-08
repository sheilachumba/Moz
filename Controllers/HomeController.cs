using ClientPortal.Models;  // Namespace for view models
using Microsoft.AspNetCore.Mvc;  // For controller base class and attributes
using Microsoft.Extensions.Logging; // For logging
using Moz.Services;   // Namespace containing the insurance service interface
using System.Diagnostics;  // For error tracing

namespace ClientPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;  // Logger instance for logging
        private readonly IInsuranceProductService _insuranceProductService;  // Service for fetching insurance products

        // Constructor with dependency injection for logger and insurance product service
        public HomeController(ILogger<HomeController> logger, IInsuranceProductService insuranceProductService)
        {
            _logger = logger;
            _insuranceProductService = insuranceProductService;
        }

        // Existing Index action
        public IActionResult Index()
        {
            return View();
        }

        // New action to get insurance products and display them
        public async Task<IActionResult> InsuranceProducts()
        {
            // Call the async service method to fetch products
            var products = await _insuranceProductService.GetInsuranceProducts();

            // Pass the products to the ViewBag for use in the view
            ViewBag.InsuranceProducts = products;

            // Return the view to render the list
            return View();
        }

        // Existing Privacy action
        public IActionResult Privacy()
        {
            return View();
        }

        // Existing error handling action
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
