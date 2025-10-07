//// Controllers/Sections.cs
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace ClientPortal.Controllers
//{
//    [Authorize]
//    public class PoliciesController : Controller
//    {
//        [HttpGet]
//        public IActionResult Index()
//        {
//            ViewData["Title"] = "Policies";
//            ViewBag.Description = "Your policies overview";
//            return View("SectionShell");
//        }
//    }

//    [Authorize]
//    public class RenewalsController : Controller
//    {
//        [HttpGet]
//        public IActionResult Index()
//        {
//            ViewData["Title"] = "Renewals";
//            ViewBag.Description = "Active and upcoming renewals";
//            return View("SectionShell");
//        }
//    }

//    [Authorize]
//    public class ClaimsController : Controller
//    {
//        [HttpGet]
//        public IActionResult Index()
//        {
//            ViewData["Title"] = "Claims";
//            ViewBag.Description = "Claims overview";
//            return View("SectionShell");
//        }
//    }

//    [Authorize]
//    public class QuotationsController : Controller
//    {
//        [HttpGet]
//        public IActionResult Index()
//        {
//            ViewData["Title"] = "Quotations";
//            ViewBag.Description = "Saved and submitted quotations";
//            return View("SectionShell");
//        }
//    }

//    [Authorize]
//    public class ProductsController : Controller
//    {
//        [HttpGet]
//        public IActionResult Index()
//        {
//            ViewData["Title"] = "Products & Services";
//            ViewBag.Description = "Available products & services";
//            return View("SectionShell");
//        }
//    }

//    [Authorize]
//    public class DocumentsController : Controller
//    {
//        [HttpGet]
//        public IActionResult Index()
//        {
//            ViewData["Title"] = "Documents";
//            ViewBag.Description = "Your documents";
//            return View("SectionShell");
//        }
//    }

//    [Authorize]
//    public class StatementsController : Controller
//    {
//        [HttpGet]
//        public IActionResult Index()
//        {
//            ViewData["Title"] = "Statements";
//            ViewBag.Description = "Statements & downloads";
//            return View("SectionShell");
//        }

//        [HttpGet]
//        public IActionResult Download(string type)
//        {
//            // Wire real download later; placeholder shell for now
//            ViewData["Title"] = "Download";
//            ViewBag.Description = $"Requested: {type}";
//            return View("SectionShell");
//        }
//    }

//    [Authorize]
//    public class InteractionsController : Controller
//    {
//        [HttpGet]
//        public IActionResult Index()
//        {
//            ViewData["Title"] = "Interactions";
//            ViewBag.Description = "Customer interactions";
//            return View("SectionShell");
//        }

//        [HttpPost]
//        public IActionResult Create([FromForm] string Subject, [FromForm] string Type, [FromForm] DateTime? When, [FromForm] string Notes)
//        {
//            // Persist later; for now just confirm and return to list
//            TempData["Ok"] = "Interaction received.";
//            return RedirectToAction(nameof(Index));
//        }
//    }

//    [Authorize]
//    public class NotificationsController : Controller
//    {
//        [HttpGet]
//        public IActionResult Index()
//        {
//            ViewData["Title"] = "Notifications";
//            ViewBag.Description = "Your notifications";
//            return View("SectionShell");
//        }
//    }

//    [Authorize]
//    public class SettingsController : Controller
//    {
//        [HttpGet]
//        public IActionResult Index()
//        {
//            ViewData["Title"] = "Settings";
//            ViewBag.Description = "Account & preferences";
//            return View("SectionShell");
//        }
//    }
//}
