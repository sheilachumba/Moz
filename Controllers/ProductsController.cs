using ClientPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Moz.Models;
using Moz.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientPortal.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IInsuranceProductService _insuranceProductService;

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
                bcProducts = await _insuranceProductService.GetInsuranceProducts();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Unable to load products at this time. Please try again later.";
                bcProducts = new List<InsuranceProduct>();
            }

            // Improved mapping for MainClass
            var cards = bcProducts.Select(p => new ProductCardVm
            {
                Id = 0,
                Name = p.Name,
                Class = GetSubClass(p), // Use improved logic below
                MainClass = MapToCanonical(p.Class_Group, p.Name)
            }).ToList();

            if (!string.IsNullOrWhiteSpace(classFilter) && !classFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                cards = cards.Where(c => c.MainClass.Equals(classFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var CanonicalKeys = new[] { "Motor", "WorkmenCompensation", "PersonalAccidents", "Engineering", "Specialized", "Multimark", "HealthLife", "Funeral", "Travel", "Property" };

            // Only group by proper subgroups (skip "Unclassified")
            var groups = cards
       .GroupBy(c => c.MainClass)
       .OrderBy(g => Array.IndexOf(CanonicalKeys, g.Key))
       .Select(g => new ProductGroupVm
       {
           MainClass = g.Key,
           SubGroups = g.GroupBy(x =>
               string.IsNullOrWhiteSpace(x.Class)
                   ? GetSubClassFromNameOrCode(x.Name, x.MainClass, x.Id)
                   : NormalizeSubClass(x.Class)
           )
           .Where(sg => !string.IsNullOrWhiteSpace(sg.Key) && sg.Key != "Unclassified")
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
            ViewBag.ErrorMessage = ViewBag.ErrorMessage;

            return View(groups);
        }

        // Improved subgroup logic: never returns "Unclassified"
        private static string GetSubClass(InsuranceProduct p)
        {
            // Try to use Class_Group if not null/empty and not a numeric code
            if (!string.IsNullOrWhiteSpace(p.Class_Group) && !p.Class_Group.StartsWith("PC"))
                return p.Class_Group;

            // Infer from product Name if possible
            if (!string.IsNullOrWhiteSpace(p.Name))
            {
                var name = p.Name.ToLowerInvariant();
                if (name.Contains("motor")) return "Motor";
                if (name.Contains("life")) return "Life";
                if (name.Contains("health")) return "Health";
                if (name.Contains("travel")) return "Travel";
                if (name.Contains("workmen")) return "Workmen Compensation";
                if (name.Contains("accident")) return "Personal Accident";
                if (name.Contains("fire")) return "Fire";
                if (name.Contains("burglary")) return "Burglary";
                if (name.Contains("cyber")) return "Cyber";
                if (name.Contains("marine")) return "Marine";
                if (name.Contains("property")) return "Property";
                if (name.Contains("houseowners") || name.Contains("householders")) return "Property";
            }

            // Fallback to Regulator_Class or Code if still blank
            if (!string.IsNullOrWhiteSpace(p.Regulator_Class))
                return p.Regulator_Class;
            if (!string.IsNullOrWhiteSpace(p.Code))
                return p.Code;

            // If truly nothing else, return "-"
            return "-";
        }

        private static string GetSubClassFromNameOrCode(string name, string mainClass, int id)
        {
            // You can improve this logic as your dataset grows
            var n = name?.ToLowerInvariant() ?? "";
            if (n.Contains("motor")) return "Motor";
            if (n.Contains("workmen")) return "WorkmenCompensation";
            if (n.Contains("accident")) return "PersonalAccidents";
            if (n.Contains("life")) return "Life";
            if (n.Contains("health")) return "Health";
            if (n.Contains("travel")) return "Travel";
            if (n.Contains("fire")) return "Fire";
            if (n.Contains("marine")) return "Marine";
            if (n.Contains("property")) return "Property";
            if (n.Contains("guarantee")) return "Guarantee";
            if (n.Contains("burglary")) return "Burglary";
            if (!string.IsNullOrWhiteSpace(mainClass)) return mainClass;
            // If all else fails, just return the name itself or the id as string
            if (!string.IsNullOrWhiteSpace(name)) return name;
            return id.ToString();
        }

        private static string MapToCanonical(string? cls, string? name)
        {
            var t = $"{cls} {name}".ToLowerInvariant();
            if (t.Contains("motor")) return "Motor";
            if (t.Contains("workmen") || t.Contains("workman")) return "WorkmenCompensation";
            if (t.Contains("personal accident") || t.Contains("gpa") || t.Contains("group personal accident")) return "PersonalAccidents";
            if (t.Contains("engineer") || t.Contains("electronic") || t.Contains("machinery")
                || t.Contains("contractor") || t.Contains("erection") || t.Contains("plant")) return "Engineering";
            if (t.Contains("multimark")) return "Multimark";
            if (t.Contains("funeral")) return "Funeral";
            if (t.Contains("health") || t.Contains("medical") || t.Contains("life") || t.Contains("health insurance")) return "HealthLife";
            if (t.Contains("special")) return "Specialized";
            if (t.Contains("travel")) return "Travel";
            if (t.Contains("buildings") || t.Contains("houseowners") || t.Contains("householders")) return "Property";
            if (t.Contains("fire") || t.Contains("perils") || t.Contains("all risks")) return "Specialized";
            if (t.Contains("aviation")) return "Specialized";
            if (t.Contains("marine")) return "Specialized";
            if (t.Contains("cyber")) return "Specialized";
            if (t.Contains("office")) return "Specialized";
            if (t.Contains("political")) return "Specialized";
            if (t.Contains("machinery breakdown")) return "Specialized";
            if (t.Contains("fidelity")) return "Specialized";
            if (t.Contains("mining")) return "Specialized";
            if (t.Contains("money all risks")) return "Specialized";
            if (t.Contains("business interruption") || t.Contains("lost profits")) return "Specialized";
            if (t.Contains("public liability")) return "Specialized";
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
