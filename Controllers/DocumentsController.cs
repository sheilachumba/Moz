using ClientPortal.Data;
using ClientPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortal.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _users;
        public DocumentsController(AppDbContext db, UserManager<ApplicationUser> users)
        { _db = db; _users = users; }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 12, CancellationToken ct = default)
        {
            var me = await _users.GetUserAsync(User);
            if (me == null) return RedirectToAction("Login", "Account");

            var baseQ = _db.Documents.Where(d => d.UserId == me.Id);
            if (!string.IsNullOrWhiteSpace(q))
                baseQ = baseQ.Where(d => d.FileName.Contains(q) || d.DocumentType.Contains(q));

            var total = await baseQ.CountAsync(ct);
            var items = await baseQ.OrderByDescending(d => d.UploadDate)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(ct);

            ViewBag.Query = q; ViewBag.Page = page; ViewBag.PageSize = pageSize; ViewBag.Total = total;
            return View(items);
        }
    }
}
