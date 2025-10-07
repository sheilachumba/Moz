using ClientPortal.Data;
using ClientPortal.Models; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ClientPortal.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        public ProductsController(AppDbContext db) => _db = db;

        private static readonly string[] CanonicalKeys = new[]
        {
            "Motor","WorkmenCompensation","PersonalAccidents","Engineering",
            "Specialized","Multimark","HealthLife","Funeral" 
        };

        private static string ImageFor(string key) => key.ToLowerInvariant() switch
        {
            "motor" => "motor",
            "workmencompensation" => "workmen-compensation",
            "personalaccidents" => "personal-accidents",
            "engineering" => "engineering",
            "specialized" => "specialized",
            "multimark" => "multimark",
            "healthlife" => "health-life",
            "funeral" => "funeral",
            _ => "placeholder"
        };

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

        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? classFilter)
        {
            var query = _db.Products.AsNoTracking().Where(p => p.Active);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLowerInvariant();
                query = query.Where(p => p.Name.ToLower().Contains(term) || p.Class.ToLower().Contains(term));
            }

            var items = await query
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name, p.Class })
                .ToListAsync();

            // Map to ClientPortal.Models.ProductCardVm
            var cards = items.Select(p =>
            {
                var main = MapToCanonical(p.Class, p.Name);
                return new ProductCardVm
                {
                    Id = p.Id,
                    Name = p.Name,
                    Class = p.Class,
                    MainClass = main // <- this exists in your Models VM
                    // ImageSlug is computed from Name in your Models VM (no need to set)
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(classFilter) && !classFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                cards = cards.Where(c => c.MainClass.Equals(classFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Group by MainClass -> then by normalized original Class
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

            return View(groups); 
        }

        private static string NormalizeSubClass(string? s)
        {
            s ??= "";
            s = s.Replace("_", " ").Trim();
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
        }
    }
}
