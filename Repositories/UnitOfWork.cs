using MOZ_UPGRADE.Context;
using MOZ_UPGRADE.Interfaces;
using MOZ_UPGRADE.Utils;
using MOZ_UPGRADE.Repositories.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MOZ_UPGRADE.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {


        private readonly IServiceScopeFactory _appDbContext;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        public UnitOfWork(IServiceScopeFactory appDbContext, 
                         CustomAuthenticationStateProvider authenticationStateProvider, 
                         HttpClient httpClient, 
                         IConfiguration configuration, 
                         ILoggerFactory loggerFactory,
                         IBusinessCentralService businessCentralService)
        {
            _appDbContext = appDbContext;
            _authenticationStateProvider = authenticationStateProvider;
            _httpClient = httpClient;
            _configuration = configuration;
            _loggerFactory = loggerFactory;
            BusinessCentralService = businessCentralService;
            userRepository = new UserRepository(appDbContext, _authenticationStateProvider);
            Get_LoggedIn_User = new GetLoggedinUser(_authenticationStateProvider);
            icolorCodes = new ColoCodesRepo();
            EmailService = new EmailService(configuration);
            TwoFactorAuthService = new TwoFactorAuthService(configuration);
            SalutationService = new SalutationService(configuration, loggerFactory.CreateLogger<SalutationService>());
            CountryRegionService = new CountryRegionService(configuration, loggerFactory.CreateLogger<CountryRegionService>());
            ReligionService = new ReligionService(configuration, loggerFactory.CreateLogger<ReligionService>());
            MeansOfIdentificationService = new MeansOfIdentificationService(configuration, loggerFactory.CreateLogger<MeansOfIdentificationService>());
            PostalCodeService = new PostalCodeService(configuration, loggerFactory.CreateLogger<PostalCodeService>());
            SourceOfIncomeService = new SourceOfIncomeService(configuration, loggerFactory.CreateLogger<SourceOfIncomeService>());
            ContactCardService = new ContactCardService(configuration, loggerFactory.CreateLogger<ContactCardService>());
            CorporateContactCardService = new CorporateContactCardService(configuration);
            ShareholderService = new ShareholderService(configuration);
            PepService = new PepService(configuration);
            GeneralDocumentService = new GeneralDocumentService(configuration);
            InsurerQuoteService = new InsurerQuoteService(configuration, httpClient);
            UnderwriterPolicyService = new UnderwriterPolicyService(configuration);
            PolicyCardService = new PolicyCardService(configuration, loggerFactory.CreateLogger<PolicyCardService>());
            DebitNoteService = new DebitNoteService(configuration, loggerFactory.CreateLogger<DebitNoteService>());
            PlanService = new PlanService(configuration, httpClient);
            CoverDetailService = new CoverDetailService(configuration, httpClient);
            LimitDetailService = new LimitDetailService(configuration);
            DeductibleService = new DeductibleService(configuration);
            PlanSelectionService = new PlanSelectionService();
            SelectedProductService = new SelectedProductService(httpClient, configuration, loggerFactory.CreateLogger<SelectedProductService>());
            QuoteContextService = new QuoteContextService();
            PostedReceiptService = new PostedReceiptService(configuration, loggerFactory.CreateLogger<PostedReceiptService>());
            ClaimsService = new ClaimsService(configuration, loggerFactory.CreateLogger<ClaimsService>());
            ComplainService = new ComplainService(configuration, loggerFactory.CreateLogger<ComplainService>());
            BusinessCentralService = businessCentralService;
        }

        public IUserRepository userRepository { get; private set; }
        public IGet_LoggedIn_User Get_LoggedIn_User { get; private set; }
        public IcolorCodes icolorCodes { get; private set; }
        public IEmailService EmailService { get; private set; }
        public ITwoFactorAuthService TwoFactorAuthService { get; private set; }
        public ISalutationService SalutationService { get; private set; }
        public ICountryRegionService CountryRegionService { get; private set; }
        public IReligionService ReligionService { get; private set; }
        public IMeansOfIdentificationService MeansOfIdentificationService { get; private set; }
        public IPostalCodeService PostalCodeService { get; private set; }
        public ISourceOfIncomeService SourceOfIncomeService { get; private set; }
        public IContactCardService ContactCardService { get; private set; }
        public ICorporateContactCardService CorporateContactCardService { get; private set; }
        public IShareholderService ShareholderService { get; private set; }
        public IPepService PepService { get; private set; }
        public IGeneralDocumentService GeneralDocumentService { get; private set; }
        public IInsurerQuoteService InsurerQuoteService { get; private set; }
        public IUnderwriterPolicyService UnderwriterPolicyService { get; private set; }
        public IPolicyCardService PolicyCardService { get; private set; }
        public IDebitNoteService DebitNoteService { get; private set; }
        public IPlanService PlanService { get; private set; }
        public ICoverDetailService CoverDetailService { get; private set; }
        public ILimitDetailService LimitDetailService { get; private set; }
        public IDeductibleService DeductibleService { get; private set; }
        public IPlanSelectionService PlanSelectionService { get; private set; }
        public ISelectedProductService SelectedProductService { get; private set; }
        public IQuoteContextService QuoteContextService { get; private set; }
        public IPostedReceiptService PostedReceiptService { get; private set; }
        public IClaimsService ClaimsService { get; private set; }
        public IComplainService ComplainService { get; private set; }
        public IBusinessCentralService BusinessCentralService { get; private set; }

        public int save()
        {
            using var scope = _appDbContext.CreateScope();
            var _contex = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return _contex.SaveChanges();
        }
        public void Dispose()
        {
            using var scope = _appDbContext.CreateScope();
            var _contex = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _contex.Dispose();
        }
    }
}
