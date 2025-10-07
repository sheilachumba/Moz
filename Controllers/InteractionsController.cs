using ClientPortal.Data;
using ClientPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortal.Controllers
{
    [Authorize]
    public class InteractionsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _users;
        public InteractionsController(AppDbContext db, UserManager<ApplicationUser> users)
        { _db = db; _users = users; }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var me = await _users.GetUserAsync(User);
            if (me == null) return RedirectToAction("Login", "Account");

            var baseQ = _db.Interactions.Where(i => i.UserId == me.Id);
            var total = await baseQ.CountAsync(ct);
            var items = await baseQ.OrderByDescending(i => i.InteractionDate)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(ct);
            ViewBag.Page = page; ViewBag.PageSize = pageSize; ViewBag.Total = total;
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Subject, string Type, DateTime? When, string? Notes, CancellationToken ct = default)
        {
            var me = await _users.GetUserAsync(User);
            if (me == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(Subject))
            {
                TempData["Err"] = "Subject is required.";
                return RedirectToAction("Index", "Dashboard");
            }

            _db.Interactions.Add(new Interaction
            {
                UserId = me.Id,
                Subject = Subject.Trim(),
                Type = string.IsNullOrWhiteSpace(Type) ? "Call" : Type.Trim(),
                InteractionDate = When ?? DateTime.UtcNow,
                Notes = Notes
            });
            await _db.SaveChangesAsync(ct);
            TempData["Ok"] = "Interaction saved.";
            return RedirectToAction("Index");
        }
    }
}
