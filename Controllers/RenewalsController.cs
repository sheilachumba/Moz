using ClientPortal.Data;
using ClientPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortal.Controllers
{
    [Authorize]
    public class RenewalsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _users;
        public RenewalsController(AppDbContext db, UserManager<ApplicationUser> users)
        { _db = db; _users = users; }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var me = await _users.GetUserAsync(User);
            if (me == null) return RedirectToAction("Login", "Account");

            var baseQ = _db.Renewals.Where(r => r.UserId == me.Id);
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                baseQ = baseQ.Where(r => r.PolicyNumber.Contains(q) || r.Status.Contains(q));
            }

            var total = await baseQ.CountAsync(ct);
            var items = await baseQ.OrderByDescending(r => r.RenewalDate)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(ct);

            ViewBag.Query = q; ViewBag.Page = page; ViewBag.PageSize = pageSize; ViewBag.Total = total;
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, CancellationToken ct = default)
        {
            var me = await _users.GetUserAsync(User);
            if (me == null) return RedirectToAction("Login", "Account");

            var ren = await _db.Renewals.FirstOrDefaultAsync(r => r.Id == id && r.UserId == me.Id, ct);
            if (ren == null) { TempData["Err"] = "Renewal not found."; return RedirectToAction(nameof(Index)); }

            ren.Status = "Completed";
            await _db.SaveChangesAsync(ct);
            TempData["Ok"] = "Renewal marked completed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
