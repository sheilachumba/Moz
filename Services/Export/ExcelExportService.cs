using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Services.Export
{
    public class ExcelExportService : IExcelExportService
    {
        public async Task<byte[]> ExportToExcel<T>(string title, List<ReportColumn> columns, List<T> data)
        {
            using (var package = new ExcelPackage())
            {
                // Create a new worksheet
                var worksheet = package.Workbook.Worksheets.Add(title);

                // Set the title
                worksheet.Cells[1, 1].Value = title;
                using (var range = worksheet.Cells[1, 1, 1, columns.Count])
                {
                    range.Merge = true;
                    range.Style.Font.Bold = true;
                    range.Style.Font.Size = 16;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Set the headers
                int row = 3;
                int col = 1;
                foreach (var column in columns)
                {
                    worksheet.Cells[row, col].Value = column.Header;
                    worksheet.Cells[row, col].Style.Font.Bold = true;
                    worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet.Column(col).AutoFit();
                    col++;
                }

                // Add the data
                row++;
                foreach (var item in data)
                {
                    col = 1;
                    var type = typeof(T);
                    foreach (var column in columns)
                    {
                        var prop = type.GetProperty(column.Property);
                        if (prop != null)
                        {
                            var value = prop.GetValue(item, null);
                            worksheet.Cells[row, col].Value = FormatValue(value, column.Format);
                        }
                        col++;
                    }
                    row++;
                }

                // Format the table
                if (data.Any())
                {
                    using (var range = worksheet.Cells[3, 1, row - 1, columns.Count])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                }

                // Auto-fit all columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Return the file content
                return await package.GetAsByteArrayAsync();
            }
        }

        public async Task<byte[]> ExportOverviewToExcel(ReportStats policyStats, ReportStats paymentStats, 
            ReportStats receiptStats, ReportStats claimStats, ReportStats quoteStats)
        {
            using (var package = new ExcelPackage())
            {
                // Create a new worksheet
                var worksheet = package.Workbook.Worksheets.Add("Overview");

                // Set the title
                worksheet.Cells[1, 1].Value = "Insurance Portfolio Overview";
                using (var range = worksheet.Cells[1, 1, 1, 2])
                {
                    range.Merge = true;
                    range.Style.Font.Bold = true;
                    range.Style.Font.Size = 16;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Add stats tables
                int row = 3;
                row = AddStatsToExcel(worksheet, "Policies", policyStats, row);
                row = AddStatsToExcel(worksheet, "Payments", paymentStats, row);
                row = AddStatsToExcel(worksheet, "Receipts", receiptStats, row);
                row = AddStatsToExcel(worksheet, "Claims", claimStats, row);
                row = AddStatsToExcel(worksheet, "Quotes", quoteStats, row);

                // Auto-fit all columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Return the file content
                return await package.GetAsByteArrayAsync();
            }
        }

        private int AddStatsToExcel(ExcelWorksheet worksheet, string title, ReportStats stats, int startRow)
        {
            // Add section title
            worksheet.Cells[startRow, 1].Value = title;
            using (var range = worksheet.Cells[startRow, 1, startRow, 2])
            {
                range.Merge = true;
                range.Style.Font.Bold = true;
                range.Style.Font.Size = 12;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }
            startRow++;

            // Add stats rows
            AddStatRow(worksheet, "Total", stats.Total, startRow++);
            if (stats.Active > 0) AddStatRow(worksheet, "Active", stats.Active, startRow++);
            if (stats.Pending > 0) AddStatRow(worksheet, "Pending", stats.Pending, startRow++);
            if (stats.Open > 0) AddStatRow(worksheet, "Open", stats.Open, startRow++);
            if (stats.Closed > 0) AddStatRow(worksheet, "Closed", stats.Closed, startRow++);
            if (stats.Converted > 0) AddStatRow(worksheet, "Converted", stats.Converted, startRow++);
            if (stats.Expired > 0) AddStatRow(worksheet, "Expired", stats.Expired, startRow++);
            if (stats.ThisMonth > 0) AddStatRow(worksheet, "This Month", stats.ThisMonth, startRow++);
            if (stats.TotalAmount > 0) AddStatRow(worksheet, "Total Amount", stats.TotalAmount, startRow++, "N2");
            if (stats.ThisMonthAmount > 0) AddStatRow(worksheet, "This Month Amount", stats.ThisMonthAmount, startRow++, "N2");

            // Add an empty row after each section
            startRow++;
            return startRow;
        }

        private void AddStatRow(ExcelWorksheet worksheet, string label, object value, int row, string format = null)
        {
            worksheet.Cells[row, 1].Value = label;
            worksheet.Cells[row, 2].Value = value;
            
            if (!string.IsNullOrEmpty(format))
            {
                worksheet.Cells[row, 2].Style.Numberformat.Format = format;
            }
        }

        private string FormatValue(object value, string format)
        {
            if (value == null) return string.Empty;

            if (string.IsNullOrEmpty(format)) return value.ToString();

            return string.Format("{0:" + format + "}", value);
        }
    }
}
