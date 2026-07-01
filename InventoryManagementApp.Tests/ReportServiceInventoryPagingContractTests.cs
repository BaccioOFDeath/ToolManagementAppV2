using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReportServiceInventoryPagingContractTests
    {
        [Fact]
        public void InventoryReportAndSummaryCountUseBoundedItemPages()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");
            var inventoryReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateInventoryReport()",
                "public async Task<FlowDocument> GenerateRentalReport(bool activeOnly = true)");
            var collectorMethod = ExtractMethod(
                source,
                "private async Task<List<ItemModel>> CollectInventoryReportItemsAsync()",
                "private async Task<int> CountItemsAsync()");
            var countMethod = ExtractMethod(
                source,
                "private async Task<int> CountItemsAsync()",
                "FlowDocument BuildReport(string title, IEnumerable<string> lines)");

            Assert.Contains("private const int InventoryReportPageSize = 500;", source, StringComparison.Ordinal);
            Assert.Contains("var items = await CollectInventoryReportItemsAsync().ConfigureAwait(false);", inventoryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("new ItemPage(1, int.MaxValue)", inventoryReport, StringComparison.Ordinal);

            AssertUsesBoundedReportPages(collectorMethod);
            AssertUsesBoundedReportPages(countMethod);
            Assert.DoesNotContain("new ItemPage(1, int.MaxValue)", collectorMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("new ItemPage(1, int.MaxValue)", countMethod, StringComparison.Ordinal);
        }

        [Fact]
        public void SummaryReportUsesCustomerCountWithoutMaterializingCustomerDirectory()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");
            var summaryReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateSummaryReport()",
                "public async Task<FlowDocument> GenerateMaintenanceReport(bool overdueOnly = false)");

            Assert.Contains("var totalCustomersTask = _customerService.CountCustomersAsync(CancellationToken.None);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var totalCustomers = await totalCustomersTask.ConfigureAwait(false);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Total Customers: {totalCustomers}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("var totalCustomersTask = _customerService.GetAllCustomersAsync();", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Total Customers: {totalCustomers.Count}\"", summaryReport, StringComparison.Ordinal);
        }

        private static void AssertUsesBoundedReportPages(string method)
        {
            Assert.Contains("var pageNumber = 1;", method, StringComparison.Ordinal);
            Assert.Contains("while (true)", method, StringComparison.Ordinal);
            Assert.Contains("var pageItemCount = 0;", method, StringComparison.Ordinal);
            Assert.Contains("new ItemPage(pageNumber, InventoryReportPageSize)", method, StringComparison.Ordinal);
            Assert.Contains("pageItemCount++;", method, StringComparison.Ordinal);
            Assert.Contains("if (pageItemCount < InventoryReportPageSize)", method, StringComparison.Ordinal);
            Assert.Contains("pageNumber++;", method, StringComparison.Ordinal);
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