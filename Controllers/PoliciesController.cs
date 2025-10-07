using ClientPortal.Data;
using ClientPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortal.Controllers
{
    [Authorize]
    public class PoliciesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _users;

        public PoliciesController(AppDbContext db, UserManager<ApplicationUser> users)
        {
            _db = db;
            _users = users;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var now = DateTime.UtcNow;
            var baseQ = _db.Policies.Where(p => p.UserId == user.Id);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                baseQ = baseQ.Where(p =>
                    p.PolicyNumber.Contains(q) ||
                    p.PolicyType.Contains(q));
            }

            var total = await baseQ.CountAsync(ct);
            var items = await baseQ
                .OrderByDescending(p => p.EndDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            ViewBag.Query = q;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;

            return View(items);
        }

        /// <summary>
        /// Handles Renew from two sources:
        ///  - If 'id' matches a Policy.Id → create a pending Renewal for that policy.
        ///  - Otherwise, if 'id' matches a Document.Id (from dashboard Expiring Documents link),
        ///    we’ll create a pending Renewal for the user’s most recent policy as a safe fallback.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Renew(int id, CancellationToken ct = default)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Try interpret 'id' as a Policy.Id first
            var policy = await _db.Policies.FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id, ct);

            if (policy == null)
            {
                // If not a policy, try interpret as a Document.Id and fallback to the latest policy
                var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id && d.UserId == user.Id, ct);

                policy = await _db.Policies
                    .Where(p => p.UserId == user.Id)
                    .OrderByDescending(p => p.EndDate)
                    .FirstOrDefaultAsync(ct);

                if (policy == null)
                {
                    TempData["Err"] = "No policy found to renew.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Create a pending renewal if one doesn't already exist after EndDate
            var exists = await _db.Renewals.AnyAsync(r =>
                    r.UserId == user.Id &&
                    r.PolicyNumber == policy.PolicyNumber &&
                    r.Status == "Pending", ct);

            if (!exists)
            {
                _db.Renewals.Add(new Renewal
                {
                    UserId = user.Id,
                    PolicyNumber = policy.PolicyNumber,
                    RenewalDate = policy.EndDate == default ? DateTime.UtcNow : policy.EndDate.AddDays(1),
                    Status = "Pending"
                });
                await _db.SaveChangesAsync(ct);
                TempData["Ok"] = $"Renewal initiated for policy {policy.PolicyNumber}.";
            }
            else
            {
                TempData["Ok"] = $"A pending renewal already exists for {policy.PolicyNumber}.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
