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
                "private async Task<int> CountItemsAsync()");

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
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}