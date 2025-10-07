using ClientPortal.Data;
using ClientPortal.Models;
using ClientPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ClientPortal.Controllers;

[Authorize]
public class KycController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly AppDbContext _db;
    private readonly IChecklistProvider _checklist;

    public KycController(UserManager<ApplicationUser> users, AppDbContext db, IChecklistProvider checklist)
    {
        _users = users; _db = db; _checklist = checklist;
    }

    // ------------------- CHECKLIST -------------------

    // GET: derive KYC type from the signed-in user
    [HttpGet]
    public async Task<IActionResult> Checklist()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        var type = user.KycType;
        var ack = await _db.ChecklistAcks.FirstOrDefaultAsync(a => a.UserId == user.Id && a.Type == type);
        if (ack != null) return RedirectToForm(type);

        var reqs = _checklist.Get(type);
        return View(new ChecklistVm(type, reqs, false));
    }

    // POST: derive KYC type from the signed-in user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checklist(bool acknowledged)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        var type = user.KycType;

        if (!acknowledged)
        {
            var reqs = _checklist.Get(type);
            ModelState.AddModelError("", "Please acknowledge the checklist & terms to continue.");
            return View(new ChecklistVm(type, reqs, false));
        }

        _db.ChecklistAcks.Add(new ChecklistAck { UserId = user.Id, Type = type });
        await _db.SaveChangesAsync();

        return RedirectToForm(type);
    }

    private IActionResult RedirectToForm(KycType type) =>
        type == KycType.Individual || type == KycType.SoleProprietor
            ? RedirectToAction(nameof(Individual))
            : RedirectToAction(nameof(Company));

    // ------------------- INDIVIDUAL (GET) -------------------

    [HttpGet]
    public async Task<IActionResult> Individual()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        // Guard: only Individual/SoleProprietor can view this form
        if (user.KycType != KycType.Individual && user.KycType != KycType.SoleProprietor)
            return Content("Invalid KYC type for Individual form (" + user.KycType + "). Contact support.");

        var kyc = await _db.IndividualKycs.SingleOrDefaultAsync(x => x.UserId == user.Id)
                  ?? new IndividualKyc
                  {
                      UserId = user.Id,
                      FirstName = user.FirstName ?? "",
                      MiddleName = user.MiddleName,
                      LastName = user.LastName ?? "",
                      Email = user.Email,
                      Phone = user.PhoneNumber
                  };

        ViewBag.Requirements = _checklist.Get(KycType.Individual);
        ViewBag.CanSubmit = await HasAllRequiredDocs(user.Id, KycType.Individual);

        ViewBag.IdentityTypes = new List<SelectListItem>
        {
            new("National ID", "National ID"),
            new("Passport", "Passport"),
            new("Birth Certificate", "Birth Certificate"),
            new("Voter Card", "Voter Card"),
            new("Driving License", "Driving License"),
            new("Work ID", "Work ID"),
            new("Foreigner Residence ID (DIRE)", "DIRE"),
            new("Refugee Identification Card", "Refugee Card"),
        };

        ViewBag.AddressTypes = new List<SelectListItem>
        {
            new("Utility Bill", "Utility Bill"),
            new("Employer Letter", "Employer Letter"),
            new("Municipal Declaration", "Municipal Declaration"),
        };

        ViewBag.IncomeProofTypes = new List<SelectListItem>
        {
            new("Employer Letter", "Employer Letter"),
            new("Self-Declaration", "Self-Declaration"),
        };

        return View(kyc);
    }

    // ------------------- INDIVIDUAL (POST) -------------------
    // Lift limits so file binding doesn't fail for larger PDFs/images.
    [HttpPost, ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = 1024L * 1024L * 100L)]
    [RequestSizeLimit(1024L * 1024L * 100L)]
    public async Task<IActionResult> Individual(
        IndividualKyc vm,
        IFormFile IdentityFile,
        IFormFile NuitFile,
        IFormFile AddressProofFile,
        IFormFile IncomeProofFile,
        IFormFile DriversLicenseFile,         // optional, used for Motor
        IFormFile PassportPhotoFile,          // optional
        [FromForm(Name = "action")] string action)   // <-- bind the clicked button ("save" | "submit")
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        // Guard: only Individual/SoleProprietor can post here
        if (user.KycType != KycType.Individual && user.KycType != KycType.SoleProprietor)
            return RedirectToAction(nameof(Company));

        // 1) Save files first so drafts never lose attachments
        await SaveFileAsync(user.Id, KycType.Individual, "IdentityDocument", IdentityFile);
        await SaveFileAsync(user.Id, KycType.Individual, "NuitDocument", NuitFile);
        await SaveFileAsync(user.Id, KycType.Individual, "AddressProof", AddressProofFile);
        await SaveFileAsync(user.Id, KycType.Individual, "IncomeProof", IncomeProofFile);
        await SaveFileAsync(user.Id, KycType.Individual, "DriversLicense", DriversLicenseFile);
        await SaveFileAsync(user.Id, KycType.Individual, "PassportPhoto", PassportPhotoFile);

        // 2) Upsert main KYC row (even if invalid -> we still redirect to dashboard)
        var existing = await _db.IndividualKycs.SingleOrDefaultAsync(x => x.UserId == user.Id);
        if (existing == null)
        {
            vm.UserId = user.Id;
            _db.IndividualKycs.Add(vm);
        }
        else
        {
            // *** KEY-SAFE UPDATE ***
            vm.Id = existing.Id;                 // keep PK
            vm.UserId = existing.UserId;         // keep FK/identifying FK
            _db.Entry(existing).CurrentValues.SetValues(vm);
        }

        try { HttpContext.Session.SetString("HasDraft", "true"); } catch { /* ignore */ }
        await _db.SaveChangesAsync();

        var isSubmit = string.Equals(action, "submit", StringComparison.OrdinalIgnoreCase);
        var isSave = string.Equals(action, "save", StringComparison.OrdinalIgnoreCase);

        if (isSubmit)
        {
            // If complete & valid -> Submitted; else keep as draft but still redirect to dashboard
            if (await HasAllRequiredDocs(user.Id, KycType.Individual) && ModelState.IsValid)
            {
                // persist status on the tracked entity (existing or vm if newly added)
                var row = existing ?? vm;
                row.Status = KycStatus.Submitted;
                row.SubmittedAt = DateTime.UtcNow;

                user.KycStatus = KycStatus.Submitted;
                await _users.UpdateAsync(user);
                await _db.SaveChangesAsync();

                TempData["Ok"] = "KYC submitted for screening.";
            }
            else
            {
                TempData["Err"] = "Some required details/documents are missing. Your draft was saved—complete it from your dashboard.";
            }

            // ✅ Always go to personalised dashboard
            return RedirectToAction("Individual", "Dashboard");
        }

        if (isSave)
        {
            TempData["Ok"] = "Draft saved. You can now access your dashboard.";
            return RedirectToAction("Individual", "Dashboard");
        }

        // Fallback: back to form (rare)
        ViewBag.Requirements = _checklist.Get(KycType.Individual);
        ViewBag.CanSubmit = await HasAllRequiredDocs(user.Id, KycType.Individual);
        return View(vm);
    }

    // ------------------- COMPANY (GET) -------------------

    [HttpGet]
    public async Task<IActionResult> Company()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        // Guard: only Company/Association can view this form
        if (user.KycType != KycType.Company && user.KycType != KycType.Association)
            return RedirectToAction(nameof(Individual));

        var kyc = await _db.CompanyKycs.SingleOrDefaultAsync(x => x.UserId == user.Id)
                  ?? new CompanyKyc
                  {
                      UserId = user.Id,
                      CompanyName = user.CompanyName ?? "",
                      ContactEmail = user.Email,
                      ContactPhone = user.PhoneNumber
                  };

        ViewBag.Requirements = _checklist.Get(KycType.Company);
        ViewBag.CanSubmit = await HasAllRequiredDocs(user.Id, KycType.Company);
        return View(kyc);
    }

    // ------------------- COMPANY (POST) -------------------
    [HttpPost, ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = 1024L * 1024L * 100L)]
    [RequestSizeLimit(1024L * 1024L * 100L)]
    public async Task<IActionResult> Company(
        CompanyKyc vm,
        IFormFile IncCertFile,     // e.g., Certificate of Incorporation
        IFormFile CR12File,        // registry doc / equivalent
        IFormFile KRAFile,         // tax PIN / equivalent for MZ adapt naming if needed
        IFormFile BoardResFile,    // board resolution / mandate
        [FromForm(Name = "action")] string action) // <-- bind the clicked button ("save" | "submit")
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        // Guard
        if (user.KycType != KycType.Company && user.KycType != KycType.Association)
            return RedirectToAction(nameof(Individual));

        // 1) Save uploaded company documents (adjust keys to your checklist keys)
        await SaveFileAsync(user.Id, KycType.Company, "CertificateOfIncorporation", IncCertFile);
        await SaveFileAsync(user.Id, KycType.Company, "CR12", CR12File);
        await SaveFileAsync(user.Id, KycType.Company, "TaxId", KRAFile);
        await SaveFileAsync(user.Id, KycType.Company, "BoardResolution", BoardResFile);

        // 2) Upsert KYC row (even if invalid -> we still redirect to dashboard)
        var existing = await _db.CompanyKycs.SingleOrDefaultAsync(x => x.UserId == user.Id);
        if (existing == null)
        {
            vm.UserId = user.Id;
            _db.CompanyKycs.Add(vm);
        }
        else
        {
            // *** KEY-SAFE UPDATE ***
            vm.Id = existing.Id;                 // keep PK
            vm.UserId = existing.UserId;         // keep FK/identifying FK
            _db.Entry(existing).CurrentValues.SetValues(vm);
        }

        await _db.SaveChangesAsync();

        var isSubmit = string.Equals(action, "submit", StringComparison.OrdinalIgnoreCase);
        var isSave = string.Equals(action, "save", StringComparison.OrdinalIgnoreCase);

        if (isSubmit)
        {
            if (await HasAllRequiredDocs(user.Id, KycType.Company) && ModelState.IsValid)
            {
                var row = existing ?? vm;
                row.Status = KycStatus.Submitted;
                row.SubmittedAt = DateTime.UtcNow;

                user.KycStatus = KycStatus.Submitted;
                await _users.UpdateAsync(user);
                await _db.SaveChangesAsync();

                TempData["Ok"] = "KYC submitted for screening.";
            }
            else
            {
                TempData["Err"] = "Some required details/documents are missing. Your draft was saved—complete it from your dashboard.";
            }
            return RedirectToAction("Company", "Dashboard");
        }

        if (isSave)
        {
            TempData["Ok"] = "Draft saved.";
            return RedirectToAction("Company", "Dashboard");
        }

        // Fallback
        ViewBag.Requirements = _checklist.Get(KycType.Company);
        ViewBag.CanSubmit = await HasAllRequiredDocs(user.Id, KycType.Company);
        return View(vm);
    }

    // ------------------- UPLOAD (AJAX helper; still available) -------------------

    [HttpPost]
    public async Task<IActionResult> Upload(KycType type, string docTypeKey, IFormFile file)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var root = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Uploads", user.Id, type.ToString());
        Directory.CreateDirectory(root);

        var safeFilename = $"{docTypeKey}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Path.GetFileName(file.FileName)}";
        var path = Path.Combine(root, safeFilename);

        using (var fs = System.IO.File.Create(path))
            await file.CopyToAsync(fs);

        var isRequired = _checklist.Get(type).FirstOrDefault(r => r.Key == docTypeKey)?.Required ?? false;

        _db.KycDocuments.Add(new KycDocument
        {
            UserId = user.Id,
            KycType = type,
            DocTypeKey = docTypeKey,
            FilePath = path,
            IsRequired = isRequired
        });
        await _db.SaveChangesAsync();

        return Ok(new { ok = true });
    }

    // ------------------- HELPERS -------------------

    private async Task SaveFileAsync(string userId, KycType type, string docTypeKey, IFormFile file)
    {
        if (file == null || file.Length == 0) return;

        var root = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Uploads", userId, type.ToString());
        Directory.CreateDirectory(root);

        var safeFileName = $"{docTypeKey}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Path.GetFileName(file.FileName)}";
        var path = Path.Combine(root, safeFileName);

        using (var fs = System.IO.File.Create(path))
            await file.CopyToAsync(fs);

        var isRequired = _checklist.Get(type).FirstOrDefault(r => r.Key == docTypeKey)?.Required ?? false;

        _db.KycDocuments.Add(new KycDocument
        {
            UserId = userId,
            KycType = type,
            DocTypeKey = docTypeKey,
            FilePath = path,
            IsRequired = isRequired
        });
    }

    private Task<bool> HasAllRequiredDocs(string userId, KycType type)
    {
        var requiredKeys = _checklist.Get(type).Where(r => r.Required).Select(r => r.Key).ToHashSet();
        var uploadedKeys = _db.KycDocuments.Where(d => d.UserId == userId && d.KycType == type)
                                           .Select(d => d.DocTypeKey).Distinct().ToHashSet();
        var allPresent = requiredKeys.IsSubsetOf(uploadedKeys);
        return Task.FromResult(allPresent);
    }
}
