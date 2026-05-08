using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IPlanSelectionService
    {
        void SetPlanData(PlanData data);
        PlanData? GetPlanData();

        void SetBeneficiaryData(SelectedProduct product);
        SelectedProduct? GetBeneficiaryData();

        void SetRenewalContext(RenewalContext context);
        RenewalContext? GetRenewalContext();
        void ClearRenewalContext();
    }

    public class PlanData
    {
        public string Description { get; set; } = string.Empty;
        public decimal PremiumRate { get; set; }
        public string PlanNo { get; set; } = string.Empty;
        public string UnderwriterCode { get; set; } = string.Empty;
        public string UnderwriterName { get; set; } = string.Empty;
        public string PlanClass { get; set; } = string.Empty;
    }

    public class RenewalContext
    {
        public bool IsRenewal { get; set; }
        public string OldPolicyNo { get; set; } = string.Empty;
    }
}
