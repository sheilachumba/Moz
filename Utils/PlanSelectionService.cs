using MOZ_UPGRADE.Interfaces;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Utils
{
    public class PlanSelectionService : IPlanSelectionService
    {
        private PlanData? _plan;
        private SelectedProduct? _beneficiary;
        private RenewalContext? _renewalContext;

        public void SetPlanData(PlanData data)
        {
            _plan = data;
        }

        public PlanData? GetPlanData()
        {
            return _plan;
        }

        public void SetBeneficiaryData(SelectedProduct product)
        {
            _beneficiary = product;
        }

        public SelectedProduct? GetBeneficiaryData()
        {
            return _beneficiary;
        }

        public void SetRenewalContext(RenewalContext context)
        {
            _renewalContext = context;
        }

        public RenewalContext? GetRenewalContext()
        {
            return _renewalContext;
        }

        public void ClearRenewalContext()
        {
            _renewalContext = null;
        }
    }
}
