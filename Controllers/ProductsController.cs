using ClientPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Moz.Models;
using Moz.Services; 

namespace ClientPortal.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IInsuranceProductService _insuranceProductService;

        // Inject the BC products service via constructor dependency injection
        public ProductsController(IInsuranceProductService insuranceProductService)
        {
            _insuranceProductService = insuranceProductService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? classFilter)
        {
            List<InsuranceProduct> bcProducts;
            try
            {
                // Try to fetch products from BC API
                bcProducts = await _insuranceProductService.GetInsuranceProducts();
            }
            catch (Exception ex)
            {
                // If an error occurs, log it and show a friendly message
                // You can use a logger here if available
                ViewBag.ErrorMessage = "Unable to load products at this time. Please try again later.";
                // Return an empty list so the view can handle it gracefully
                bcProducts = new List<InsuranceProduct>();
            }

            // ... (rest of your mapping, filtering, grouping logic as before)

            // (same as previous step)
            var cards = bcProducts.Select(p => new ProductCardVm
            {
                Id = 0,
                Name = p.Name,
                Class = p.Class_Group,
                MainClass = MapToCanonical(p.Class_Group, p.Name)
            }).ToList();

            if (!string.IsNullOrWhiteSpace(classFilter) && !classFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                cards = cards.Where(c => c.MainClass.Equals(classFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var CanonicalKeys = new[] { "Motor", "WorkmenCompensation", "PersonalAccidents", "Engineering", "Specialized", "Multimark", "HealthLife", "Funeral" };

            var groups = cards
                .GroupBy(c => c.MainClass)
                .OrderBy(g => Array.IndexOf(CanonicalKeys, g.Key))
                .Select(g => new ProductGroupVm
                {
                    MainClass = g.Key,
                    SubGroups = g.GroupBy(x => string.IsNullOrWhiteSpace(x.Class) ? "Unclassified" : NormalizeSubClass(x.Class))
                                 .OrderBy(sg => sg.Key)
                                 .Select(sg => new ProductSubGroupVm
                                 {
                                     SubClass = sg.Key,
                                     Items = sg.OrderBy(x => x.Name).ToList()
                                 })
                                 .ToList()
                })
                .ToList();

            ViewBag.Query = q;
            ViewBag.Classes = new[] { "All" }.Concat(CanonicalKeys).ToArray();
            ViewBag.ClassFilter = classFilter ?? "All";

            // Pass any error message to the view
            ViewBag.ErrorMessage = ViewBag.ErrorMessage;

            return View(groups);
        }


        // Copy your MapToCanonical and NormalizeSubClass private methods here (same as before)
        private static string MapToCanonical(string? cls, string? name)
        {
            var t = $"{cls} {name}".ToLowerInvariant();
            if (t.Contains("motor")) return "Motor";
            if (t.Contains("workmen") || t.Contains("workman")) return "WorkmenCompensation";
            if (t.Contains("personal accident") || t.Contains("gpa")) return "PersonalAccidents";
            if (t.Contains("engineer") || t.Contains("electronic") || t.Contains("machinery")
                || t.Contains("contractor") || t.Contains("erection") || t.Contains("plant")) return "Engineering";
            if (t.Contains("multimark")) return "Multimark";
            if (t.Contains("funeral")) return "Funeral";
            if (t.Contains("health") || t.Contains("medical") || t.Contains("life")) return "HealthLife";
            if (t.Contains("special")) return "Specialized";
            return "Specialized";
        }

        private static string NormalizeSubClass(string? s)
        {
            s ??= "";
            s = s.Replace("_", " ").Trim();
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
        }
    }
}
