using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ClientPortal.Controllers
{
    [Authorize]
    public class StatementsController : Controller
    {
        // Stub: returns a small text file so your buttons work end-to-end.
        // Replace with real PDF generation later.
        [HttpGet]
        public IActionResult Download(string type)
        {
            var safe = string.IsNullOrWhiteSpace(type) ? "Account" : type;
            var bytes = Encoding.UTF8.GetBytes($"Statement type: {safe}\nGenerated: {DateTime.UtcNow:O}");
            var fileName = $"{safe}-Statement-{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
            return File(bytes, "text/plain", fileName);
        }

        [HttpGet]
        public IActionResult Index() => RedirectToAction("Download", new { type = "Account" });
    }
}
