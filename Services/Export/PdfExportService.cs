using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Services.Export
{
    public class PdfExportService : IPdfExportService
    {
        public async Task<byte[]> ExportToPdf<T>(string title, List<ReportColumn> columns, List<T> data)
        {
            try
            {
                // Register fonts (only needs to be done once, but safe to call multiple times)
                QuestPDF.Settings.License = LicenseType.Community;
                
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Page settings
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        // Header
                        page.Header().Column(column =>
                        {
                            column.Item().Row(row =>
                            {
                                row.RelativeItem()
                                    .Text(title)
                                    .Bold()
                                    .FontSize(16)
                                    .FontColor(Colors.Blue.Darken2);

                                row.RelativeItem()
                                    .AlignRight()
                                    .Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                                    .FontSize(9);
                            });
                            
                            column.Item().PaddingBottom(10);
                            column.Item().BorderBottom(1);
                        });

                        // Content
                        page.Content().Column(column =>
                        {
                            if (data == null || !data.Any())
                            {
                                column.Item().PaddingTop(20).Text("No data available for export.")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Medium);
                                return;
                            }

                            // Table
                            column.Item().Table(table =>
                            {
                                // Set column widths
                                var columnWidths = columns.Select(c => 
                                    c.Property.Contains("Name", StringComparison.OrdinalIgnoreCase) || 
                                    c.Property.Contains("Description", StringComparison.OrdinalIgnoreCase) || 
                                    c.Property.Contains("PolicyNumber", StringComparison.OrdinalIgnoreCase) ? 3f :
                                    c.Property.Contains("Date", StringComparison.OrdinalIgnoreCase) ? 2f : 1f).ToArray();
                                
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    foreach (var width in columnWidths)
                                    {
                                        columns.RelativeColumn(width);
                                    }
                                });

                                // Header row
                                foreach (var column in columns)
                                {
                                    table.Cell()
                                        .Background(Colors.Blue.Medium)
                                        .Padding(5)
                                        .Text(column.Header)
                                        .FontColor(Colors.White)
                                        .FontSize(9)
                                        .Bold();
                                }

                                // Data rows
                                var rowIndex = 0;
                                foreach (var item in data)
                                {
                                    rowIndex++;
                                    var type = typeof(T);
                                    
                                    foreach (var column in columns)
                                    {
                                        var prop = type.GetProperty(column.Property);
                                        var value = prop?.GetValue(item);
                                        var formattedValue = FormatValue(value, column.Format);
                                        
                                        table.Cell()
                                            .Background(rowIndex % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White)
                                            .Padding(5)
                                            .Text(formattedValue ?? "N/A")
                                            .FontSize(9);
                                    }
                                }
                            });
                        });

                        // Footer
                        page.Footer().Column(column =>
                        {
                            column.Item().AlignRight().Text(text =>
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });
                        });
                    });
                });

                // Generate PDF
                using var stream = new MemoryStream();
                document.GeneratePdf(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF Generation Error: {ex}");
                throw new InvalidOperationException($"Failed to generate PDF: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> ExportOverviewToPdf(ReportStats policyStats, ReportStats paymentStats, 
            ReportStats receiptStats, ReportStats claimStats, ReportStats quoteStats)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;
                
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // Content
                        page.Content().Column(column =>
                        {
                            // Header
                            column.Item().Row(row =>
                            {
                                row.RelativeItem()
                                    .Text("Dashboard Overview")
                                    .FontSize(16)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                row.RelativeItem()
                                    .AlignRight()
                                    .Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                                    .FontSize(9);
                            });
                            
                            column.Item().PaddingBottom(10);
                            column.Item().BorderBottom(1);
                            column.Item().PaddingBottom(10);
                            
                            // Add stats sections
                            AddStatsSection(column, "Policies", policyStats);
                            column.Item().Height(15);
                            AddStatsSection(column, "Payments", paymentStats);
                            column.Item().Height(15);
                            AddStatsSection(column, "Receipts", receiptStats);
                            column.Item().Height(15);
                            AddStatsSection(column, "Claims", claimStats);
                            column.Item().Height(15);
                            AddStatsSection(column, "Quotes", quoteStats);
                        });

                        // Footer
                        page.Footer()
                            .BorderTop(1)
                            .PaddingTop(5)
                            .DefaultTextStyle(TextStyle.Default.FontSize(8))
                            .Text(text =>
                            {
                                text.AlignCenter();
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                                text.Span("    |    ");
                                text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                            });
                    });
                });

                // Generate PDF
                using var stream = new MemoryStream();
                document.GeneratePdf(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF Overview Generation Error: {ex}");
                throw new InvalidOperationException($"Failed to generate overview PDF: {ex.Message}", ex);
            }
        }

        private void AddStatsSection(ColumnDescriptor column, string title, ReportStats stats)
        {
            if (stats == null) return;

            column.Item().Column(innerColumn =>
            {
                innerColumn.Item().Text(title)
                    .FontSize(12)
                    .Bold();
                
                innerColumn.Item().PaddingBottom(5);
                
                innerColumn.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    AddStatRow(table, "Total", stats.Total);
                    AddStatRow(table, "Active", stats.Active);
                    AddStatRow(table, "Pending", stats.Pending);
                    AddStatRow(table, "Open", stats.Open);
                    AddStatRow(table, "Closed", stats.Closed);
                    AddStatRow(table, "Converted", stats.Converted);
                    AddStatRow(table, "Expired", stats.Expired);
                    AddStatRow(table, "This Month", stats.ThisMonth);
                    
                    if (stats.TotalAmount != 0)
                        AddStatRow(table, "Total Amount", stats.TotalAmount, "N");
                        
                    if (stats.ThisMonthAmount != 0)
                        AddStatRow(table, "This Month Amount", stats.ThisMonthAmount, "N");
                });
            });
        }

        private void AddStatRow(TableDescriptor table, string label, object value, string format = null)
        {
            table.Cell().Text(label).FontSize(9);
            table.Cell().Text(FormatValue(value, format)).FontSize(9);
        }

        private string FormatValue(object value, string format)
        {
            if (value == null) return string.Empty;
            if (string.IsNullOrEmpty(format)) return value.ToString();

            try
            {
                if (value is IFormattable formattable)
                {
                    return formattable.ToString(format, null);
                }
                return value.ToString();
            }
            catch
            {
                return value.ToString();
            }
        }
    }
}
