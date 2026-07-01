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
                "private static string FormatLimitedCount(int count)");

            Assert.Contains("private const int InventoryReportPageSize = 500;", source, StringComparison.Ordinal);
            Assert.Contains("var items = await CollectInventoryReportItemsAsync().ConfigureAwait(false);", inventoryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("new ItemPage(1, int.MaxValue)", inventoryReport, StringComparison.Ordinal);

            AssertUsesBoundedReportPages(collectorMethod);
            AssertUsesBoundedReportPages(countMethod);
            Assert.DoesNotContain("new ItemPage(1, int.MaxValue)", collectorMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("new ItemPage(1, int.MaxValue)", countMethod, StringComparison.Ordinal);
        }

        [Fact]
        public void SummaryReportUsesCountApisWithoutMaterializingCappedDirectories()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");
            var summaryReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateSummaryReport()",
                "public async Task<FlowDocument> GenerateMaintenanceReport(bool overdueOnly = false)");

            Assert.Contains("var totalRentalsTask = _rentalService.CountRentalsAsync();", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var totalActiveRentalsTask = _rentalService.CountActiveRentalsAsync();", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var totalCustomersTask = _customerService.CountCustomersAsync(CancellationToken.None);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var totalUsersTask = _userService.CountUsersAsync(CancellationToken.None);", summaryReport, StringComparison.Ordinal);

            Assert.Contains("var totalRentals = await totalRentalsTask.ConfigureAwait(false);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var totalActiveRentals = await totalActiveRentalsTask.ConfigureAwait(false);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var totalCustomers = await totalCustomersTask.ConfigureAwait(false);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var totalUsers = await totalUsersTask.ConfigureAwait(false);", summaryReport, StringComparison.Ordinal);

            Assert.Contains("$\"Total Rentals (History): {totalRentals}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Active Rentals: {totalActiveRentals}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Total Customers: {totalCustomers}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Total Users: {totalUsers}\"", summaryReport, StringComparison.Ordinal);

            Assert.DoesNotContain("var totalRentalsTask = _rentalService.GetAllRentalsAsync();", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("var totalActiveRentalsTask = _rentalService.GetActiveRentalsAsync();", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("var totalCustomersTask = _customerService.GetAllCustomersAsync();", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("var totalUsersTask = _userService.GetAllUsersAsync(CancellationToken.None);", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Total Rentals (History): {totalRentals.Count}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Active Rentals: {totalActiveRentals.Count}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Total Customers: {totalCustomers.Count}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Total Users: {totalUsers.Count}\"", summaryReport, StringComparison.Ordinal);
        }

        [Fact]
        public void CappedDetailedReportsDisclosePotentiallyTruncatedRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");
            var rentalReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateRentalReport(bool activeOnly = true)",
                "public async Task<FlowDocument> GenerateRentalFrequencyReport(int topN = 20)");
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
            var limitNotice = ExtractMethod(
                source,
                "private static string FormatLimitedCount(int count)",
                "FlowDocument BuildReport(string title, IEnumerable<string> lines)");

            Assert.Contains("private const int DetailedReportResultLimit = 500;", source, StringComparison.Ordinal);
            Assert.Contains("AddReportLimitNotice(lines, rentals.Count, \"rental records\")", rentalReport, StringComparison.Ordinal);
            Assert.Contains("AddReportLimitNotice(lines, customers.Count, \"customers\")", customerReport, StringComparison.Ordinal);
            Assert.Contains("AddReportLimitNotice(lines, users.Count, \"users\")", userReport, StringComparison.Ordinal);
            Assert.Contains("AddReportLimitNotice(lines, records.Count, \"maintenance records\")", maintenanceReport, StringComparison.Ordinal);
            Assert.Contains("AddReportLimitNotice(lines, records.Count, \"calibration records\")", calibrationReport, StringComparison.Ordinal);
            Assert.Contains("AddReportLimitNotice(lines, reservations.Count, \"reservations\")", reservationReport, StringComparison.Ordinal);
            Assert.Contains("AddReportLimitNotice(lines, kits.Count, \"active kits\")", kitReport, StringComparison.Ordinal);
            Assert.Contains("var itemCount = FormatLimitedCount(items.Count);", kitReport, StringComparison.Ordinal);

            Assert.Contains("count >= DetailedReportResultLimit ? $\"{DetailedReportResultLimit}+\" : count.ToString();", limitNotice, StringComparison.Ordinal);
            Assert.Contains("This report shows the first {DetailedReportResultLimit}", limitNotice, StringComparison.Ordinal);
            Assert.Contains("Use filters or exports for a narrower full-detail review.", limitNotice, StringComparison.Ordinal);
        }

        [Fact]
        public void SummaryOptionalCountsShowWhenDirectoryCapsMayApply()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");
            var summaryReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateSummaryReport()",
                "public async Task<FlowDocument> GenerateMaintenanceReport(bool overdueOnly = false)");

            Assert.Contains("$\"Overdue Maintenance: {FormatLimitedCount(overdueMaintenance.Count)}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Upcoming Maintenance (30 days): {FormatLimitedCount(upcomingMaintenance.Count)}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Overdue Calibrations: {FormatLimitedCount(overdueCalibration.Count)}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Upcoming Calibrations (30 days): {FormatLimitedCount(upcomingCalibration.Count)}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Active Reservations: {FormatLimitedCount(activeReservations.Count)}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Upcoming Reservations (7 days): {FormatLimitedCount(upcomingReservations.Count)}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Active Kits: {FormatLimitedCount(activeKits.Count)}\"", summaryReport, StringComparison.Ordinal);

            Assert.DoesNotContain("$\"Overdue Maintenance: {overdueMaintenance.Count}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Upcoming Maintenance (30 days): {upcomingMaintenance.Count}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Overdue Calibrations: {overdueCalibration.Count}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Upcoming Calibrations (30 days): {upcomingCalibration.Count}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Active Reservations: {activeReservations.Count}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Upcoming Reservations (7 days): {upcomingReservations.Count}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Active Kits: {activeKits.Count}\"", summaryReport, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalServiceProvidesVisibleTotalRentalCountForSummaryReports()
        {
            var interfaceSource = ReadRepoFile("InventoryManagementApp", "Interfaces", "IRentalService.cs");
            var rentalServiceSource = ReadRepoFile("InventoryManagementApp", "Services", "Rentals", "RentalService.cs");
            var countMethod = ExtractMethod(
                rentalServiceSource,
                "public async Task<int> CountRentalsAsync()",
                "public async Task<int> CountActiveRentalsAsync()");

            Assert.Contains("Task<int> CountRentalsAsync();", interfaceSource, StringComparison.Ordinal);
            Assert.Contains("SELECT COUNT(r.RentalID)", countMethod, StringComparison.Ordinal);
            Assert.Contains("FROM Rentals r", countMethod, StringComparison.Ordinal);
            Assert.Contains("JOIN Items t ON r.ItemID = t.ItemID", countMethod, StringComparison.Ordinal);
            Assert.Contains("JOIN Customers c ON r.CustomerID = c.CustomerID", countMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT @RentalListLimit", countMethod, StringComparison.Ordinal);
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