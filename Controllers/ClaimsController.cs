using ClientPortal.Data;
using ClientPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortal.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _users;

        public ClaimsController(AppDbContext db, UserManager<ApplicationUser> users)
        {
            _db = db;
            _users = users;
        }

        // GET /Claims
        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? status, int page = 1, int pageSize = 10)
        {
            var me = await _users.GetUserAsync(User);
            if (me == null) return Challenge();

            var query = _db.Claims.AsNoTracking().Where(c => c.UserId == me.Id);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(c =>
                    c.ClaimNumber.Contains(q) ||
                    c.PolicyNumber.Contains(q) ||
                    c.Description.Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c => c.Status == status);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.ClaimDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Query = q;
            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;

            // View expects IEnumerable<ClientPortal.Models.Claim>
            return View(items);
        }

        // GET /Claims/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var me = await _users.GetUserAsync(User);
            if (me == null) return Challenge();

            var policyNumbers = await _db.Policies
                .AsNoTracking()
                .Where(p => p.UserId == me.Id)
                .Select(p => p.PolicyNumber)
                .Distinct()
                .ToListAsync();

            var vm = new ClientPortal.Models.ClaimCreateVm
            {
                PolicyNumbers = policyNumbers
            };

            // View expects ClientPortal.Models.ClaimCreateVm
            return View(vm);
        }

        // POST /Claims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientPortal.Models.ClaimCreateVm vm)
        {
            var me = await _users.GetUserAsync(User);
            if (me == null) return Challenge();

            if (!ModelState.IsValid)
            {
                vm.PolicyNumbers = await _db.Policies
                    .AsNoTracking()
                    .Where(p => p.UserId == me.Id)
                    .Select(p => p.PolicyNumber)
                    .Distinct()
                    .ToListAsync();

                return View(vm); // still ClientPortal.Models.ClaimCreateVm
            }

            var claim = new Claim
            {
                UserId = me.Id,
                ClaimNumber = $"CLM-{DateTime.UtcNow:yyyyMMddHHmmss}",
                PolicyNumber = vm.PolicyNumber ?? "",
                Status = "Pending",
                ClaimDate = vm.DateOfOccurrence ?? DateTime.UtcNow.Date,
                Amount = vm.Amount ?? 0m,
                Description = vm.LossDescription ?? ""
            };

            _db.Claims.Add(claim);
            await _db.SaveChangesAsync();

            // TODO: push to ERP here when ready

            TempData["ok"] = "Claim submitted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
