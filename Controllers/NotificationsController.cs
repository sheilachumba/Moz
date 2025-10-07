using ClientPortal.Data;
using ClientPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientPortal.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _users;
        public NotificationsController(AppDbContext db, UserManager<ApplicationUser> users)
        { _db = db; _users = users; }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct = default)
        {
            var me = await _users.GetUserAsync(User);
            if (me == null) return RedirectToAction("Login", "Account");

            var items = await _db.Notifications
                .Where(n => n.UserId == me.Id)
                .OrderByDescending(n => n.CreatedDate)
                .Take(50)
                .ToListAsync(ct);

            // Mark all as read when visiting the page
            foreach (var n in items.Where(x => !x.IsRead)) n.IsRead = true;
            await _db.SaveChangesAsync(ct);

            return View(items);
        }
    }
}
