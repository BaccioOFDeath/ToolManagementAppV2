using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReportServiceUserFacingLabelContractTests
    {
        [Fact]
        public void InventoryAndRentalReportsUseUserFacingItemLabels()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");

            var inventoryReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateInventoryReport()",
                "public async Task<FlowDocument> GenerateRentalReport(bool activeOnly = true)");
            var rentalReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateRentalReport(bool activeOnly = true)",
                "public async Task<FlowDocument> GenerateRentalFrequencyReport(int topN = 20)");

            Assert.Contains("BuildReport(\"Inventory Report\", lines)", inventoryReport, StringComparison.Ordinal);
            Assert.Contains("Item ID: {t.ItemID}", inventoryReport, StringComparison.Ordinal);
            Assert.Contains("Item Number: {t.ItemNumber}", inventoryReport, StringComparison.Ordinal);
            Assert.Contains("Quantity: {t.QuantityOnHand}", inventoryReport, StringComparison.Ordinal);
            Assert.Contains("Item ID: {r.ItemID}", rentalReport, StringComparison.Ordinal);

            Assert.DoesNotContain("ItemModel Inventory Report", inventoryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("ItemModel ID:", inventoryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("ItemModel ID:", rentalReport, StringComparison.Ordinal);
            Assert.DoesNotContain("ItemNumber:", inventoryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("Qty:", inventoryReport, StringComparison.Ordinal);
        }

        [Fact]
        public void GeneratedReportsUseReadableIdLabels()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");

            var activityLogReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateActivityLogReport()",
                "public async Task<FlowDocument> GenerateCustomerReport()");
            var customerReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateCustomerReport()",
                "public async Task<FlowDocument> GenerateUserReport()");
            var userReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateUserReport()",
                "public async Task<FlowDocument> GenerateSummaryReport()");
            var maintenanceReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateMaintenanceReport(bool overdueOnly = false)",
                "public async Task<FlowDocument> GenerateCalibrationReport(bool overdueOnly = false)");
            var calibrationReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateCalibrationReport(bool overdueOnly = false)",
                "public async Task<FlowDocument> GenerateReservationReport(bool activeOnly = true)");
            var reservationReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateReservationReport(bool activeOnly = true)",
                "public async Task<FlowDocument> GenerateKitReport()");
            var kitReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateKitReport()",
                "private async Task<List<ItemModel>> CollectInventoryReportItemsAsync()");

            Assert.Contains("Log ID: {l.LogID}", activityLogReport, StringComparison.Ordinal);
            Assert.Contains("User ID: {l.UserID}", activityLogReport, StringComparison.Ordinal);
            Assert.Contains("Customer ID: {c.CustomerID}", customerReport, StringComparison.Ordinal);
            Assert.Contains("User ID: {u.UserID}", userReport, StringComparison.Ordinal);
            Assert.Contains("User Name: {u.UserName}", userReport, StringComparison.Ordinal);
            Assert.Contains("Admin: {u.IsAdmin}", userReport, StringComparison.Ordinal);
            Assert.Contains("Maintenance ID: {m.MaintenanceID}", maintenanceReport, StringComparison.Ordinal);
            Assert.Contains("Calibration ID: {c.CalibrationID}", calibrationReport, StringComparison.Ordinal);
            Assert.Contains("Certificate Number: {c.CertificateNumber}", calibrationReport, StringComparison.Ordinal);
            Assert.Contains("Reservation ID: {r.ReservationID}", reservationReport, StringComparison.Ordinal);
            Assert.Contains("Quantity: {r.Quantity}", reservationReport, StringComparison.Ordinal);
            Assert.Contains("Kit Number: {kit.KitNumber}", kitReport, StringComparison.Ordinal);
            Assert.Contains("Kit Name: {kit.Name}", kitReport, StringComparison.Ordinal);
            Assert.Contains("Item Count: {itemCount}", kitReport, StringComparison.Ordinal);

            Assert.DoesNotContain("LogID:", activityLogReport, StringComparison.Ordinal);
            Assert.DoesNotContain("UserID:", activityLogReport, StringComparison.Ordinal);
            Assert.DoesNotContain("CustomerID:", customerReport, StringComparison.Ordinal);
            Assert.DoesNotContain("UserID:", userReport, StringComparison.Ordinal);
            Assert.DoesNotContain("UserName:", userReport, StringComparison.Ordinal);
            Assert.DoesNotContain("IsAdmin:", userReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"ID: {m.MaintenanceID}", maintenanceReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"ID: {c.CalibrationID}", calibrationReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"ID: {r.ReservationID}", reservationReport, StringComparison.Ordinal);
            Assert.DoesNotContain("Cert#:", calibrationReport, StringComparison.Ordinal);
            Assert.DoesNotContain("Qty:", reservationReport, StringComparison.Ordinal);
            Assert.DoesNotContain("Kit: {kit.KitNumber}", kitReport, StringComparison.Ordinal);
            Assert.DoesNotContain("Items: {itemCount}", kitReport, StringComparison.Ordinal);
        }

        [Fact]
        public void BuildReportAddsReadableEmptyStateWhenNoRowsExist()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");

            var buildReport = ExtractMethod(
                source,
                "FlowDocument BuildReport(string title, IEnumerable<string> lines)",
                "    }\n}");

            Assert.Contains("var reportLines = lines as IReadOnlyCollection<string> ?? lines.ToList();", buildReport, StringComparison.Ordinal);
            Assert.Contains("if (reportLines.Count == 0)", buildReport, StringComparison.Ordinal);
            Assert.Contains("No report records found.", buildReport, StringComparison.Ordinal);
            Assert.True(
                buildReport.IndexOf("if (reportLines.Count == 0)", StringComparison.Ordinal) <
                buildReport.IndexOf("foreach (var line in reportLines)", StringComparison.Ordinal),
                "Expected empty report output to be filled before report paragraphs are rendered.");
        }

        [Fact]
        public void BuildReportUsesProfessionalPrintPreviewLayout()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");

            var buildReport = ExtractMethod(
                source,
                "FlowDocument BuildReport(string title, IEnumerable<string> lines)",
                "private static void AddReportLine(FlowDocument doc, string line)");
            var addReportLine = ExtractMethod(
                source,
                "private static void AddReportLine(FlowDocument doc, string line)",
                "    }\n}");

            Assert.Contains("PagePadding = new Thickness(48, 40, 48, 40)", buildReport, StringComparison.Ordinal);
            Assert.Contains("MinPageWidth = 720", buildReport, StringComparison.Ordinal);
            Assert.Contains("MaxPageWidth = 960", buildReport, StringComparison.Ordinal);
            Assert.Contains("ColumnWidth = double.PositiveInfinity", buildReport, StringComparison.Ordinal);
            Assert.DoesNotContain("PageWidth = 800", buildReport, StringComparison.Ordinal);

            Assert.Contains("var preparedAt = DateTime.Now;", buildReport, StringComparison.Ordinal);
            Assert.Contains("$\"Prepared {preparedAt:yyyy-MM-dd HH:mm}\"", buildReport, StringComparison.Ordinal);
            Assert.Contains("new Run(\"End of report\")", buildReport, StringComparison.Ordinal);
            Assert.Contains("AddReportLine(doc, line);", buildReport, StringComparison.Ordinal);

            Assert.Contains("line.StartsWith(\"Note:\", StringComparison.Ordinal)", addReportLine, StringComparison.Ordinal);
            Assert.Contains("paragraph.FontStyle = FontStyles.Italic;", addReportLine, StringComparison.Ordinal);
            Assert.Contains("paragraph.Foreground = Brushes.DarkSlateGray;", addReportLine, StringComparison.Ordinal);
            Assert.Contains("paragraph.Background = Brushes.AliceBlue;", addReportLine, StringComparison.Ordinal);
        }

        private static string ExtractMethod(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find method start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find method end marker: {endMarker}");

            return source[start..end];
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }
}