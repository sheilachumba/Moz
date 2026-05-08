using MOZ_UPGRADE.Utils;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IUnitOfWork:IDisposable
    {
        IUserRepository userRepository { get; }
        IGet_LoggedIn_User Get_LoggedIn_User { get; }
        IcolorCodes icolorCodes { get; }
        IEmailService EmailService { get; }
        ITwoFactorAuthService TwoFactorAuthService { get; }
        ISalutationService SalutationService { get; }
        ICountryRegionService CountryRegionService { get; }
        IReligionService ReligionService { get; }
        IMeansOfIdentificationService MeansOfIdentificationService { get; }
        IPostalCodeService PostalCodeService { get; }
        ISourceOfIncomeService SourceOfIncomeService { get; }
        IContactCardService ContactCardService { get; }
        ICorporateContactCardService CorporateContactCardService { get; }
        IShareholderService ShareholderService { get; }
        IPepService PepService { get; }
        IGeneralDocumentService GeneralDocumentService { get; }
        IInsurerQuoteService InsurerQuoteService { get; }
        IUnderwriterPolicyService UnderwriterPolicyService { get; }
        IPolicyCardService PolicyCardService { get; }
        IDebitNoteService DebitNoteService { get; }
        IPlanService PlanService { get; }
        ICoverDetailService CoverDetailService { get; }
        ILimitDetailService LimitDetailService { get; }
        IDeductibleService DeductibleService { get; }
        IPlanSelectionService PlanSelectionService { get; }
        ISelectedProductService SelectedProductService { get; }
        IQuoteContextService QuoteContextService { get; }
        IPostedReceiptService PostedReceiptService { get; }
        IClaimsService ClaimsService { get; }
        IComplainService ComplainService { get; }
        IBusinessCentralService BusinessCentralService { get; }
        
        int save();
    }

   
}
