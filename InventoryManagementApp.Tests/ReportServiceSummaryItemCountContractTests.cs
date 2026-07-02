using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReportServiceSummaryItemCountContractTests
    {
        [Fact]
        public void SummaryReportUsesItemCountApiForInventoryTotal()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");
            var summaryReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateSummaryReport()",
                "public async Task<FlowDocument> GenerateMaintenanceReport(bool overdueOnly = false)");

            Assert.Contains("var totalItemsTask = _itemService.CountItemsAsync(new ItemFilter(null, SortField.Name, SortDirection.Ascending, false), CancellationToken.None);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var totalItems = await totalItemsTask.ConfigureAwait(false);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Total Items: {totalItems}\"", summaryReport, StringComparison.Ordinal);

            Assert.DoesNotContain("var totalItemsTask = CountItemsAsync();", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("new ItemPage(", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("GetItemsAsync(", summaryReport, StringComparison.Ordinal);
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
