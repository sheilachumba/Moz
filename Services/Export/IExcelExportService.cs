using System.Collections.Generic;
using System.Threading.Tasks;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Services.Export
{
    public interface IExcelExportService
    {
        Task<byte[]> ExportToExcel<T>(string title, List<ReportColumn> columns, List<T> data);
        Task<byte[]> ExportOverviewToExcel(ReportStats policyStats, ReportStats paymentStats, ReportStats receiptStats, 
                                         ReportStats claimStats, ReportStats quoteStats);
    }
}
