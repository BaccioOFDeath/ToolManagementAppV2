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
            Assert.Contains("Item ID: {r.ItemID}", rentalReport, StringComparison.Ordinal);

            Assert.DoesNotContain("ItemModel Inventory Report", inventoryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("ItemModel ID:", inventoryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("ItemModel ID:", rentalReport, StringComparison.Ordinal);
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
