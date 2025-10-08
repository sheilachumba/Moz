using ClientPortal.Data;
using ClientPortal.Models;
using ClientPortal.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moz.Services;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// === SQL Server  ===
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    opts.SignIn.RequireConfirmedEmail = true;
    opts.Password.RequiredLength = 12;
    opts.Password.RequireDigit = true;
    opts.Password.RequireLowercase = true;
    opts.Password.RequireUppercase = true;
    opts.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "keys"))) // e.g. /Moz/keys
    .SetApplicationName("ClientPortal"); // keep this constant across runs/hosts

// Email + Checklist services
builder.Services.AddTransient<IEmailSenderEx, SmtpEmailSender>();
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<IChecklistProvider, InMemoryChecklistProvider>();
builder.Services.AddTransient<IBank_interface, Bankservice>();
builder.Services.AddTransient<IInsuranceProductService, InsuranceProductService>();
builder.Services.AddTransient<BusinessCentralBasicApiService>();
builder.Services.AddTransient<KycSubmissionService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Auto-migrate on startup (dev-friendly)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

// ===================== KYC gate (locks app pre-submission) =====================
app.Use(async (ctx, next) =>
{
    if (ctx.User?.Identity?.IsAuthenticated == true)
    {

        var path = (ctx.Request.Path.Value ?? string.Empty).ToLowerInvariant();

        bool whitelisted =
            path.StartsWith("/kyc") ||
            path.StartsWith("/account") ||
            path.StartsWith("/identity") ||
            path.StartsWith("/css") ||
            path.StartsWith("/js") ||
            path.StartsWith("/lib") ||
            path.StartsWith("/images") ||
            path == "/" || path.StartsWith("/home") ||
            path.StartsWith("/dashboard") ||
            path.StartsWith("/policies") ||
            path.StartsWith("/renewals") ||
            path.StartsWith("/claims") ||
            path.StartsWith("/quotations") ||
            path.StartsWith("/documents") ||
            path.StartsWith("/statements") ||
            path.StartsWith("/interactions") ||
            path.StartsWith("/notifications") ||
        path.StartsWith("/products") ||     // ? add this
    path.StartsWith("/settings");

        if (!whitelisted)
        {
            var userMgr = ctx.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var me = await userMgr.GetUserAsync(ctx.User);

            if (me != null && me.KycStatus < KycStatus.Submitted)
            {
                var type = me.AccountType == AccountType.Individual ? "Individual" : "Company";
                ctx.Response.Redirect($"/Kyc/Checklist?type={type}");
                return;
            }
        }
    }

    await next();
});
// ===============================================================================

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
