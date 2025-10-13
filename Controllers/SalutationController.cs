using ClientPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Moz.Controllers
{
    
        public class SalutationController : Controller
        {
            private readonly SalutationService _service = new SalutationService();

            public async Task<ActionResult> Index()
            {
                var salutations = await _service.GetSalutationsAsync();
            ViewBag.Salutations = new SelectList(salutations, "Code", "Description");
            return View();
            }
        }
    }

