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
    private readonly KycSubmissionService _kycSubmissionService;
    private readonly SalutationService _salutationService;
    //private readonly CountryRegionService _countryRegionService;
    //private readonly PostalCodeService _postalCodeService;
    //private readonly SourceOfFundsService _sourceOfFundsService;
    //private readonly MeansOfIdentificationService _meansOfIdentificationService;
    
    public KycController(
        UserManager<ApplicationUser> users,
        AppDbContext db,
        IChecklistProvider checklist,
        KycSubmissionService kycSubmissionService,
        SalutationService salutationService)
        //CountryRegionService countryRegionService,
        //PostalCodeService postalCodeService,
        //SourceOfFundsService sourceOfFundsService,
        //MeansOfIdentificationService meansOfIdentificationService
    {
        _users = users;
        _db = db;
        _checklist = checklist;
        _kycSubmissionService = kycSubmissionService;
        _salutationService = salutationService;
        //_countryRegionService = countryRegionService;
        //_postalCodeService = postalCodeService;
        //_sourceOfFundsService = sourceOfFundsService;
        //_meansOfIdentificationService = meansOfIdentificationService;
    }

    // ------------------- CHECKLIST -------------------

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

        // Fetch salutations
        var salutations = await _salutationService.GetSalutationsAsync();
        ViewBag.IdentityTypes = salutations.Select(s => new SelectListItem
        {
            Text = string.IsNullOrEmpty(s.Description) ? s.Code : $"{s.Code} - {s.Description}",
            Value = s.Code
        }).ToList();

        var selectedSalutation = kyc.IdType;
        foreach (var item in ViewBag.IdentityTypes)
        {
            if (item.Value == selectedSalutation)
            {
                item.Selected = true;
                break;
            }
        }
        kyc.SelectedSalutation = selectedSalutation;

        // Fetch countries
        //var countries = await _countryRegionService.GetCountriesAsync();
        //ViewBag.Countries = countries.Select(c =>
        //    new SelectListItem
        //    {
        //        Text = c.Name,
        //        Value = c.Code
        //    }).ToList();

        // Fetch postal codes
        //var postalcodes = await _postalCodeService.GetPostalCodesAsync();
        //ViewBag.PostalCodes = postalcodes
        //    .Select(pc => new SelectListItem
        //    {
        //        Text = $"{pc.Code} - {pc.City}",
        //        Value = pc.Code
        //    }).ToList();

        // Fetch sources of funds
        //var sources = await _sourceOfFundsService.GetSourcesOfFundsAsync();
        //ViewBag.SourceOfFunds = sources
        //    .Select(sf => new SelectListItem
        //    {
        //        Text = sf.Source,
        //        Value = sf.Source
        //    }).ToList();

        // Fetch means of identification
        //var meansOfIdList = await _meansOfIdentificationService.GetMeansOfIdentificationAsync();
        //ViewBag.MeansOfIdentification = meansOfIdList.Select(m =>
        //    new SelectListItem
        //    {
        //        Text = string.IsNullOrWhiteSpace(m.Description) ? m.Means_of_ID : $"{m.Means_of_ID} - {m.Description}",
        //        Value = m.Means_of_ID
        //    }).ToList();

        // Static dropdowns
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

    [HttpPost, ValidateAntiForgeryToken]
    [RequestFormLimits(MultipartBodyLengthLimit = 1024L * 1024L * 100L)]
    [RequestSizeLimit(1024L * 1024L * 100L)]
    public async Task<IActionResult> Individual(
        IndividualKyc vm,
        IFormFile IdentityFile,
        IFormFile NuitFile,
        IFormFile AddressProofFile,
        IFormFile IncomeProofFile,
        IFormFile DriversLicenseFile,
        IFormFile PassportPhotoFile,
        [FromForm(Name = "action")] string action)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        if (user.KycType != KycType.Individual && user.KycType != KycType.SoleProprietor)
            return RedirectToAction(nameof(Company));

        vm.IdType = vm.SelectedSalutation;

        await SaveFileAsync(user.Id, KycType.Individual, "IdentityDocument", IdentityFile);
        await SaveFileAsync(user.Id, KycType.Individual, "NuitDocument", NuitFile);
        await SaveFileAsync(user.Id, KycType.Individual, "AddressProof", AddressProofFile);
        await SaveFileAsync(user.Id, KycType.Individual, "IncomeProof", IncomeProofFile);
        await SaveFileAsync(user.Id, KycType.Individual, "DriversLicense", DriversLicenseFile);
        await SaveFileAsync(user.Id, KycType.Individual, "PassportPhoto", PassportPhotoFile);

        var existing = await _db.IndividualKycs.SingleOrDefaultAsync(x => x.UserId == user.Id);
        if (existing == null)
        {
            vm.UserId = user.Id;
            _db.IndividualKycs.Add(vm);
        }
        else
        {
            vm.Id = existing.Id;
            vm.UserId = existing.UserId;
            _db.Entry(existing).CurrentValues.SetValues(vm);
        }

        try { HttpContext.Session.SetString("HasDraft", "true"); } catch { }
        await _db.SaveChangesAsync();

        var isSubmit = string.Equals(action, "submit", StringComparison.OrdinalIgnoreCase);
        var isSave = string.Equals(action, "save", StringComparison.OrdinalIgnoreCase);

        if (isSubmit)
        {
            if (await HasAllRequiredDocs(user.Id, KycType.Individual) && ModelState.IsValid)
            {
                try
                {
                    bool success = await _kycSubmissionService.SubmitIndividualKycAsync(vm);

                    if (!success)
                    {
                        TempData["Err"] = "Failed to submit KYC data to Business Central.";
                        return RedirectToAction("Individual", "Dashboard");
                    }

                    var insuredCardNo = "INSU000"; // Replace with actual insured card number from BC response if available
                    var uploadSuccess = await _kycSubmissionService.UploadKycDocumentAsync(insuredCardNo, IdentityFile.FileName, IdentityFile.FileName);
                    if (!uploadSuccess)
                    {
                        TempData["Err"] = "KYC data submitted but failed to upload some documents.";
                        return RedirectToAction("Individual", "Dashboard");
                    }

                    var row = existing ?? vm;
                    row.Status = KycStatus.Submitted;
                    row.SubmittedAt = DateTime.UtcNow;
                    user.KycStatus = KycStatus.Submitted;
                    await _users.UpdateAsync(user);
                    await _db.SaveChangesAsync();

                    TempData["Ok"] = "KYC submitted successfully for screening.";
                }
                catch (Exception ex)
                {
                    TempData["Err"] = $"An error occurred during submission: {ex.Message}";
                }
            }
            else
            {
                TempData["Err"] = "Some required details/documents are missing. Your draft was saved—complete it from your dashboard.";
            }
            return RedirectToAction("Individual", "Dashboard");
        }

        if (isSave)
        {
            TempData["Ok"] = "Draft saved.";
            return RedirectToAction("Individual", "Dashboard");
        }

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
        IFormFile IncCertFile,
        IFormFile CR12File,
        IFormFile KRAFile,
        IFormFile BoardResFile,
        [FromForm(Name = "action")] string action)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Challenge();

        if (user.KycType != KycType.Company && user.KycType != KycType.Association)
            return RedirectToAction(nameof(Individual));

        await SaveFileAsync(user.Id, KycType.Company, "CertificateOfIncorporation", IncCertFile);
        await SaveFileAsync(user.Id, KycType.Company, "CR12", CR12File);
        await SaveFileAsync(user.Id, KycType.Company, "TaxId", KRAFile);
        await SaveFileAsync(user.Id, KycType.Company, "BoardResolution", BoardResFile);

        var existing = await _db.CompanyKycs.SingleOrDefaultAsync(x => x.UserId == user.Id);
        if (existing == null)
        {
            vm.UserId = user.Id;
            _db.CompanyKycs.Add(vm);
        }
        else
        {
            vm.Id = existing.Id;
            vm.UserId = existing.UserId;
            _db.Entry(existing).CurrentValues.SetValues(vm);
        }

        await _db.SaveChangesAsync();

        var isSubmit = string.Equals(action, "submit", StringComparison.OrdinalIgnoreCase);
        var isSave = string.Equals(action, "save", StringComparison.OrdinalIgnoreCase);

        if (isSubmit)
        {
            if (await HasAllRequiredDocs(user.Id, KycType.Company) && ModelState.IsValid)
            {
                try
                {
                    var success = await _kycSubmissionService.SubmitCompanyKycAsync(vm);
                    if (!success)
                    {
                        TempData["Err"] = "Failed to submit KYC data to Business Central. Please try again later.";
                        return RedirectToAction("Company", "Dashboard");
                    }

                    var insuredCardNo = "INSU000"; // Replace with actual insured card number from BC response if available
                    var uploadSuccess = await _kycSubmissionService.UploadKycDocumentAsync(insuredCardNo, IncCertFile.FileName, IncCertFile.FileName);
                    if (!uploadSuccess)
                    {
                        TempData["Err"] = "KYC data submitted but failed to upload some documents.";
                        return RedirectToAction("Company", "Dashboard");
                    }

                    var row = existing ?? vm;
                    row.Status = KycStatus.Submitted;
                    row.SubmittedAt = DateTime.UtcNow;
                    user.KycStatus = KycStatus.Submitted;
                    await _users.UpdateAsync(user);
                    await _db.SaveChangesAsync();

                    TempData["Ok"] = "KYC submitted successfully for screening.";
                }
                catch (Exception ex)
                {
                    TempData["Err"] = $"An error occurred during submission: {ex.Message}";
                }
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

        ViewBag.Requirements = _checklist.Get(KycType.Company);
        ViewBag.CanSubmit = await HasAllRequiredDocs(user.Id, KycType.Company);
        return View(vm);
    }

    // ------------------- UPLOAD (AJAX helper) -------------------

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
