using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReportServiceSummaryAndKitContractTests
    {
        [Fact]
        public void SummaryReportStartsOptionalCountReadsBeforeSingleAwait()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");
            var summaryReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateSummaryReport()",
                "public async Task<FlowDocument> GenerateMaintenanceReport(bool overdueOnly = false)");

            Assert.Contains("var overdueMaintenanceTask = _maintenanceService?.CountOverdueMaintenanceAsync();", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var upcomingMaintenanceTask = _maintenanceService?.CountUpcomingMaintenanceAsync(30);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var overdueCalibrationTask = _calibrationService?.CountOverdueCalibrationAsync();", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var upcomingCalibrationTask = _calibrationService?.CountUpcomingCalibrationAsync(30);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var activeReservationsTask = _reservationService?.CountActiveReservationsAsync();", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var upcomingReservationsTask = _reservationService?.CountUpcomingReservationsAsync(7);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var activeKitsTask = _kitService?.CountActiveKitsAsync();", summaryReport, StringComparison.Ordinal);

            Assert.Contains("var summaryTasks = new List<Task>", summaryReport, StringComparison.Ordinal);
            Assert.Contains("AddIfNotNull(summaryTasks, overdueMaintenanceTask);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("AddIfNotNull(summaryTasks, upcomingMaintenanceTask);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("AddIfNotNull(summaryTasks, overdueCalibrationTask);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("AddIfNotNull(summaryTasks, upcomingCalibrationTask);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("AddIfNotNull(summaryTasks, activeReservationsTask);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("AddIfNotNull(summaryTasks, upcomingReservationsTask);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("AddIfNotNull(summaryTasks, activeKitsTask);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("await Task.WhenAll(summaryTasks).ConfigureAwait(false);", summaryReport, StringComparison.Ordinal);

            Assert.DoesNotContain("await Task.WhenAll(overdueMaintenanceTask, upcomingMaintenanceTask)", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("await Task.WhenAll(overdueCalibrationTask, upcomingCalibrationTask)", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("await Task.WhenAll(activeReservationsTask, upcomingReservationsTask)", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("var activeKits = await _kitService.CountActiveKitsAsync().ConfigureAwait(false);", summaryReport, StringComparison.Ordinal);
        }

        [Fact]
        public void KitReportUsesGroupedItemCountsInsteadOfLoadingEachKitItemList()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");
            var kitReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateKitReport()",
                "private async Task<List<ItemModel>> CollectInventoryReportItemsAsync()");

            Assert.Contains("var kitsTask = _kitService.GetActiveKitsAsync();", kitReport, StringComparison.Ordinal);
            Assert.Contains("var totalActiveKitsTask = _kitService.CountActiveKitsAsync();", kitReport, StringComparison.Ordinal);
            Assert.Contains("await Task.WhenAll(kitsTask, totalActiveKitsTask).ConfigureAwait(false);", kitReport, StringComparison.Ordinal);
            Assert.Contains("var itemCounts = await _kitService.CountKitItemsByKitIdsAsync(kits.Select(kit => kit.KitID)).ConfigureAwait(false);", kitReport, StringComparison.Ordinal);
            Assert.Contains("itemCounts.TryGetValue(kit.KitID, out var count)", kitReport, StringComparison.Ordinal);
            Assert.Contains("? FormatLimitedCount(count)", kitReport, StringComparison.Ordinal);
            Assert.Contains(": \"0\";", kitReport, StringComparison.Ordinal);
            Assert.Contains("AddExactReportLimitNotice(lines, kits.Count, totalActiveKits, \"active kits\")", kitReport, StringComparison.Ordinal);

            Assert.DoesNotContain("await _kitService.GetKitItemsAsync(kit.KitID)", kitReport, StringComparison.Ordinal);
            Assert.DoesNotContain("items.Count", kitReport, StringComparison.Ordinal);
        }

        [Fact]
        public void KitServiceProvidesGroupedItemCountQueryForReportRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Kits", "KitService.cs");
            var countMethod = ExtractMethod(
                source,
                "public async Task<Dictionary<int, int>> CountKitItemsByKitIdsAsync(IEnumerable<int> kitIds)",
                "/// <summary>\n        /// Retrieves a specific kit by its ID.");

            Assert.Contains("using System.Linq;", source, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(kitIds));", countMethod, StringComparison.Ordinal);
            Assert.Contains("var distinctKitIds = kitIds.Distinct().ToList();", countMethod, StringComparison.Ordinal);
            Assert.Contains("distinctKitIds.Any(id => id < 1)", countMethod, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentOutOfRangeException(nameof(kitIds), \"Kit IDs must be greater than 0.\");", countMethod, StringComparison.Ordinal);
            Assert.Contains("return new Dictionary<int, int>();", countMethod, StringComparison.Ordinal);
            Assert.Contains("var counts = distinctKitIds.ToDictionary(id => id, _ => 0);", countMethod, StringComparison.Ordinal);
            Assert.Contains("SELECT KitID, COUNT(KitItemID) AS ItemCount", countMethod, StringComparison.Ordinal);
            Assert.Contains("FROM KitItems", countMethod, StringComparison.Ordinal);
            Assert.Contains("WHERE KitID IN ({string.Join(\", \", parameterNames)})", countMethod, StringComparison.Ordinal);
            Assert.Contains("GROUP BY KitID", countMethod, StringComparison.Ordinal);
            Assert.Contains("cmd.Parameters.AddWithValue(parameterNames[index], distinctKitIds[index]);", countMethod, StringComparison.Ordinal);
            Assert.Contains("counts[kitId] = Convert.ToInt32(reader[\"ItemCount\"] ?? 0);", countMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("reader.GetInt32(reader.GetOrdinal(\"ItemCount\"))", countMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT @KitItemListLimit", countMethod, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportServiceKeepsNullableTaskHelperLocalAndTiny()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");
            var helper = ExtractMethod(
                source,
                "private static void AddIfNotNull(List<Task> tasks, Task? task)",
                "private static IEnumerable<string> AddExactReportLimitNotice");

            Assert.Contains("if (task != null)", helper, StringComparison.Ordinal);
            Assert.Contains("tasks.Add(task);", helper, StringComparison.Ordinal);
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
