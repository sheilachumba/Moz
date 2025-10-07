using ClientPortal.Data;
using ClientPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortal.Controllers
{
    [Authorize]
    public class QuotationsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _users;
        public QuotationsController(AppDbContext db, UserManager<ApplicationUser> users)
        { _db = db; _users = users; }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? status, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var me = await _users.GetUserAsync(User);
            if (me == null) return RedirectToAction("Login", "Account");

            var baseQ = _db.Quotations.Where(x => x.UserId == me.Id);
            if (!string.IsNullOrWhiteSpace(q))
                baseQ = baseQ.Where(x => x.Reference.Contains(q));
            if (!string.IsNullOrWhiteSpace(status))
                baseQ = baseQ.Where(x => x.Status == status);

            var total = await baseQ.CountAsync(ct);
            var items = await baseQ.OrderByDescending(x => x.UpdatedAt)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(ct);

            ViewBag.Query = q; ViewBag.Status = status; ViewBag.Page = page; ViewBag.PageSize = pageSize; ViewBag.Total = total;
            return View(items);
        }
    }
}
