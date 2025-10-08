using ClientPortal.Data;
using ClientPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moz.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientPortal.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly AppDbContext _db;

        public DashboardController(
     UserManager<ApplicationUser> users,
     AppDbContext db,
     IInsuranceProductService insuranceProductService)
        {
            _users = users;
            _db = db;
            _insuranceProductService = insuranceProductService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (IsSubmittedOrVerified(user))
            {
                if (user.KycType is KycType.Individual or KycType.SoleProprietor)
                    return RedirectToAction(nameof(Individual));
                if (user.KycType is KycType.Company or KycType.Association)
                    return RedirectToAction(nameof(Company));
            }

            await PopulateDashboardDataAsync(user);
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Individual()
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (user.KycType is not (KycType.Individual or KycType.SoleProprietor))
                return RedirectToAction(nameof(Index));

            if (!IsSubmittedOrVerified(user))
            {
                var hasDraft =
                    await _db.IndividualKycs.AnyAsync(x => x.UserId == user.Id) ||
                    await _db.KycDocuments.AnyAsync(d => d.UserId == user.Id && d.KycType == KycType.Individual);

                if (!hasDraft)
                    return RedirectToAction(nameof(Index));
            }

            await PopulateDashboardDataAsync(user);
            ViewBag.KycStatus = user.KycStatus;
            ViewBag.CoverUrl = null;
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Company()
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (user.KycType is not (KycType.Company or KycType.Association))
                return RedirectToAction(nameof(Index));

            if (!IsSubmittedOrVerified(user))
            {
                var hasDraft =
                    await _db.CompanyKycs.AnyAsync(x => x.UserId == user.Id) ||
                    await _db.KycDocuments.AnyAsync(d => d.UserId == user.Id && d.KycType == KycType.Company);

                if (!hasDraft)
                    return RedirectToAction(nameof(Index));
            }

            await PopulateDashboardDataAsync(user);
            ViewBag.KycStatus = user.KycStatus;
            ViewBag.CoverUrl = null;
            return View(user);
        }
        private readonly IInsuranceProductService _insuranceProductService;

        private static bool IsSubmittedOrVerified(ApplicationUser user) =>
            user.KycStatus is KycStatus.Submitted or KycStatus.Verified || user.KycCompleted;

        private async Task PopulateDashboardDataAsync(ApplicationUser user)
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            user.ActivePoliciesCount = await _db.Policies.CountAsync(p =>
                p.UserId == user.Id &&
                p.Status == "Active" &&
                p.StartDate <= now &&
                p.EndDate >= now);

            user.ExpiredPoliciesCount = await _db.Policies.CountAsync(p =>
                p.UserId == user.Id &&
                p.EndDate < now);

            user.NewPoliciesThisMonth = await _db.Policies.CountAsync(p =>
                p.UserId == user.Id &&
                p.StartDate >= monthStart);

            user.ActiveRenewalsCount = await _db.Renewals.CountAsync(r =>
                r.UserId == user.Id &&
                (r.Status == "Pending" || r.Status == "InProgress"));

            user.PendingClaimsCount = await _db.Claims.CountAsync(c =>
                c.UserId == user.Id &&
                (c.Status == "Pending" || c.Status == "Submitted" || c.Status == "InReview"));

            var claimDurations = await _db.Claims
                .Where(c => c.UserId == user.Id && (c.Status == "Approved" || c.Status == "Closed" || c.Status == "Rejected"))
                .Select(c => EF.Functions.DateDiffDay(c.ClaimDate, c.UpdatedAt))
                .ToListAsync();

            user.AverageClaimProcessingDays = claimDurations.Count > 0
                ? (int)Math.Round(claimDurations.Average())
                : 0;

            user.TotalProductsCount = await _db.Products.CountAsync(p => p.Active);

            user.SubscribedServicesCount = await _db.Policies
                .Where(p => p.UserId == user.Id && p.Status == "Active")
                .Select(p => p.PolicyType)
                .Distinct()
                .CountAsync();

            // LIVE PRODUCT LIST
            var apiProducts = await _insuranceProductService.GetInsuranceProducts();
            var products = apiProducts.Select(p => new Product
            {
                Id = 0, // or map if you have an Id
                Name = p.Name,
                Class = p.Class_Group,
                Active = true // or map accordingly
            }).ToList();

            user.TotalProductsCount = products.Count;
            ViewBag.LiveProducts = products;


            user.UnreadNotificationsCount = await _db.Notifications.CountAsync(n =>
                n.UserId == user.Id && !n.IsRead);

            var recentClaims = await _db.Claims
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.UpdatedAt)
                .Take(5)
                .Select(c => new ClaimQuotationVm
                {
                    Type = "Claim",
                    Reference = c.ClaimNumber,
                    Status = c.Status,
                    StatusClass = c.Status == "Approved" ? "ok" :
                                  (c.Status == "Pending" || c.Status == "InReview" || c.Status == "Submitted") ? "warn" :
                                  (c.Status == "Rejected" ? "bad" : ""),
                    UpdatedDate = c.UpdatedAt
                })
                .ToListAsync();

            var recentQuotes = await _db.Quotations
                .Where(q => q.UserId == user.Id)
                .OrderByDescending(q => q.UpdatedAt)
                .Take(5)
                .Select(q => new ClaimQuotationVm
                {
                    Type = "Quotation",
                    Reference = q.Reference,
                    Status = q.Status,
                    StatusClass = q.Status == "Accepted" ? "ok" :
                                  q.Status == "Declined" ? "bad" : "warn",
                    UpdatedDate = q.UpdatedAt
                })
                .ToListAsync();

            var recentMerged = recentClaims
                .Concat(recentQuotes)
                .OrderByDescending(x => x.UpdatedDate)
                .Take(5)
                .ToList();

            ViewBag.RecentClaimsQuotations = recentMerged.Count > 0 ? recentMerged : null;

            var expiringDocs = await _db.Documents
                .Where(d => d.UserId == user.Id && d.ExpiryDate != null && d.ExpiryDate >= now)
                .OrderBy(d => d.ExpiryDate)
                .Take(5)
                .Select(d => new ExpiringDocumentVm
                {
                    Id = d.Id,
                    Name = d.FileName,
                    ExpiryDate = d.ExpiryDate!.Value,
                    UrgencyClass = d.ExpiryDate!.Value <= now.AddDays(7) ? "bad" :
                                   d.ExpiryDate!.Value <= now.AddDays(30) ? "warn" : "ok"
                })
                .ToListAsync();

            ViewBag.ExpiringDocuments = expiringDocs.Count > 0 ? expiringDocs : null;
        }
    }
}
