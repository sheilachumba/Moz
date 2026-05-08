
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MOZ_UPGRADE.Context;

using MOZ_UPGRADE.Interfaces;
using MOZ_UPGRADE.Repositories;
using MOZ_UPGRADE.Services;
using MOZ_UPGRADE.Services.Export;
using MOZ_UPGRADE.Utils;
using MudBlazor.Services;
using StackExchange.Redis;
using Stripe;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IInsurerQuoteService, InsurerQuoteService>();
builder.Services.AddScoped<MOZ_UPGRADE.Utils.IEmailService, MOZ_UPGRADE.Utils.EmailService>();
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});
builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddAuthorization();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddCircuitOptions(options =>
{
    options.DetailedErrors = true;
});
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddScoped<ISharePointDocumentService, MOZ_UPGRADE.Repositories.Services.SharePointDocumentService>();
builder.Services.AddScoped<KycUploadContext>();

// Register services
builder.Services.AddScoped<MOZ_UPGRADE.Utils.LanguageService>();
builder.Services.AddScoped<IBusinessCentralService, MOZ_UPGRADE.Repositories.Services.BusinessCentralService>();

// Register export services
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
builder.Services.AddControllers();

var key = Encoding.ASCII.GetBytes("YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv");
builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
   .AddJwtBearer(options =>
   {
       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuer = true,
           ValidateAudience = true,
           ValidateLifetime = true,
           ValidateIssuerSigningKey = true,
           ValidIssuer = "MyIssuer",
           ValidAudience = "MyAudience",
           IssuerSigningKey = new SymmetricSecurityKey(key)
       };
   });

string? connectionString = "Server=localhost;Database=MOZ_UPGRADE;port=3306;Uid=root;Pwd=123456";
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddMudServices();


builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));

builder.Services.AddHostedService<RedisStartupLoader>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseSession();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
