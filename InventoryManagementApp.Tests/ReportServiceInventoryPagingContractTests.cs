using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReportServiceInventoryPagingContractTests
    {
        [Fact]
        public void InventoryReportUsesBoundedItemPagesWithoutObsoleteCountHelper()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");
            var inventoryReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateInventoryReport()",
                "public async Task<FlowDocument> GenerateRentalReport(bool activeOnly = true)");
            var collectorMethod = ExtractMethod(
                source,
                "private async Task<List<ItemModel>> CollectInventoryReportItemsAsync()",
                "private static string FormatLimitedCount(int count)");

            Assert.Contains("private const int InventoryReportPageSize = 500;", source, StringComparison.Ordinal);
            Assert.Contains("var items = await CollectInventoryReportItemsAsync().ConfigureAwait(false);", inventoryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("new ItemPage(1, int.MaxValue)", inventoryReport, StringComparison.Ordinal);

            AssertUsesBoundedReportPages(collectorMethod);
            Assert.DoesNotContain("new ItemPage(1, int.MaxValue)", collectorMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("private async Task<int> CountItemsAsync()", source, StringComparison.Ordinal);
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
        public void CappedDetailedReportsUseParallelExactTruncationCounts()
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
            Assert.DoesNotContain("AddReportLimitNotice", source, StringComparison.Ordinal);

            Assert.Contains("var rentalsTask = activeOnly", rentalReport, StringComparison.Ordinal);
            Assert.Contains("? _rentalService.GetActiveRentalsAsync()", rentalReport, StringComparison.Ordinal);
            Assert.Contains(": _rentalService.GetAllRentalsAsync();", rentalReport, StringComparison.Ordinal);
            Assert.Contains("var totalRentalsTask = activeOnly", rentalReport, StringComparison.Ordinal);
            Assert.Contains("? _rentalService.CountActiveRentalsAsync()", rentalReport, StringComparison.Ordinal);
            Assert.Contains(": _rentalService.CountRentalsAsync();", rentalReport, StringComparison.Ordinal);
            Assert.Contains("await Task.WhenAll(rentalsTask, totalRentalsTask).ConfigureAwait(false);", rentalReport, StringComparison.Ordinal);
            Assert.Contains("var rentals = await rentalsTask.ConfigureAwait(false);", rentalReport, StringComparison.Ordinal);
            Assert.Contains("var totalRentals = await totalRentalsTask.ConfigureAwait(false);", rentalReport, StringComparison.Ordinal);
            Assert.Contains("AddExactReportLimitNotice(lines, rentals.Count, totalRentals, \"rental records\")", rentalReport, StringComparison.Ordinal);

            Assert.Contains("var customersTask = _customerService.SearchCustomersAsync(string.Empty, CancellationToken.None);", customerReport, StringComparison.Ordinal);
            Assert.Contains("var totalCustomersTask = _customerService.CountCustomersAsync(CancellationToken.None);", customerReport, StringComparison.Ordinal);
            Assert.Contains("await Task.WhenAll(customersTask, totalCustomersTask).ConfigureAwait(false);", customerReport, StringComparison.Ordinal);
            Assert.Contains("var customers = await customersTask.ConfigureAwait(false);", customerReport, StringComparison.Ordinal);
            Assert.Contains("var totalCustomers = await totalCustomersTask.ConfigureAwait(false);", customerReport, StringComparison.Ordinal);
            Assert.Contains("AddExactReportLimitNotice(lines, customers.Count, totalCustomers, \"customers\")", customerReport, StringComparison.Ordinal);
            Assert.DoesNotContain("_customerService.GetAllCustomersAsync()", customerReport, StringComparison.Ordinal);
            Assert.DoesNotContain("customers.Take(DetailedReportResultLimit)", customerReport, StringComparison.Ordinal);

            Assert.Contains("var usersTask = _userService.GetAllUsersAsync(CancellationToken.None);", userReport, StringComparison.Ordinal);
            Assert.Contains("var totalUsersTask = _userService.CountUsersAsync(CancellationToken.None);", userReport, StringComparison.Ordinal);
            Assert.Contains("await Task.WhenAll(usersTask, totalUsersTask).ConfigureAwait(false);", userReport, StringComparison.Ordinal);
            Assert.Contains("var users = await usersTask.ConfigureAwait(false);", userReport, StringComparison.Ordinal);
            Assert.Contains("var totalUsers = await totalUsersTask.ConfigureAwait(false);", userReport, StringComparison.Ordinal);
            Assert.Contains("AddExactReportLimitNotice(lines, users.Count, totalUsers, \"users\")", userReport, StringComparison.Ordinal);

            Assert.Contains("var recordsTask = overdueOnly", maintenanceReport, StringComparison.Ordinal);
            Assert.Contains("? _maintenanceService.GetOverdueMaintenanceAsync()", maintenanceReport, StringComparison.Ordinal);
            Assert.Contains(": _maintenanceService.GetAllMaintenanceRecordsAsync();", maintenanceReport, StringComparison.Ordinal);
            Assert.Contains("var totalRecordsTask = overdueOnly", maintenanceReport, StringComparison.Ordinal);
            Assert.Contains("? _maintenanceService.CountOverdueMaintenanceAsync()", maintenanceReport, StringComparison.Ordinal);
            Assert.Contains(": _maintenanceService.CountMaintenanceRecordsAsync();", maintenanceReport, StringComparison.Ordinal);
            Assert.Contains("await Task.WhenAll(recordsTask, totalRecordsTask).ConfigureAwait(false);", maintenanceReport, StringComparison.Ordinal);
            Assert.Contains("var records = await recordsTask.ConfigureAwait(false);", maintenanceReport, StringComparison.Ordinal);
            Assert.Contains("var totalRecords = await totalRecordsTask.ConfigureAwait(false);", maintenanceReport, StringComparison.Ordinal);
            Assert.Contains("AddExactReportLimitNotice(lines, records.Count, totalRecords, \"maintenance records\")", maintenanceReport, StringComparison.Ordinal);

            Assert.Contains("var recordsTask = overdueOnly", calibrationReport, StringComparison.Ordinal);
            Assert.Contains("? _calibrationService.GetOverdueCalibrationAsync()", calibrationReport, StringComparison.Ordinal);
            Assert.Contains(": _calibrationService.GetAllCalibrationRecordsAsync();", calibrationReport, StringComparison.Ordinal);
            Assert.Contains("var totalRecordsTask = overdueOnly", calibrationReport, StringComparison.Ordinal);
            Assert.Contains("? _calibrationService.CountOverdueCalibrationAsync()", calibrationReport, StringComparison.Ordinal);
            Assert.Contains(": _calibrationService.CountCalibrationRecordsAsync();", calibrationReport, StringComparison.Ordinal);
            Assert.Contains("await Task.WhenAll(recordsTask, totalRecordsTask).ConfigureAwait(false);", calibrationReport, StringComparison.Ordinal);
            Assert.Contains("var records = await recordsTask.ConfigureAwait(false);", calibrationReport, StringComparison.Ordinal);
            Assert.Contains("var totalRecords = await totalRecordsTask.ConfigureAwait(false);", calibrationReport, StringComparison.Ordinal);
            Assert.Contains("AddExactReportLimitNotice(lines, records.Count, totalRecords, \"calibration records\")", calibrationReport, StringComparison.Ordinal);

            Assert.Contains("var reservationsTask = activeOnly", reservationReport, StringComparison.Ordinal);
            Assert.Contains("? _reservationService.GetActiveReservationsAsync()", reservationReport, StringComparison.Ordinal);
            Assert.Contains(": _reservationService.GetAllReservationsAsync();", reservationReport, StringComparison.Ordinal);
            Assert.Contains("var totalReservationsTask = activeOnly", reservationReport, StringComparison.Ordinal);
            Assert.Contains("? _reservationService.CountActiveReservationsAsync()", reservationReport, StringComparison.Ordinal);
            Assert.Contains(": _reservationService.CountReservationsAsync();", reservationReport, StringComparison.Ordinal);
            Assert.Contains("await Task.WhenAll(reservationsTask, totalReservationsTask).ConfigureAwait(false);", reservationReport, StringComparison.Ordinal);
            Assert.Contains("var reservations = await reservationsTask.ConfigureAwait(false);", reservationReport, StringComparison.Ordinal);
            Assert.Contains("var totalReservations = await totalReservationsTask.ConfigureAwait(false);", reservationReport, StringComparison.Ordinal);
            Assert.Contains("AddExactReportLimitNotice(lines, reservations.Count, totalReservations, \"reservations\")", reservationReport, StringComparison.Ordinal);

            Assert.Contains("var kitsTask = _kitService.GetActiveKitsAsync();", kitReport, StringComparison.Ordinal);
            Assert.Contains("var totalActiveKitsTask = _kitService.CountActiveKitsAsync();", kitReport, StringComparison.Ordinal);
            Assert.Contains("await Task.WhenAll(kitsTask, totalActiveKitsTask).ConfigureAwait(false);", kitReport, StringComparison.Ordinal);
            Assert.Contains("var kits = await kitsTask.ConfigureAwait(false);", kitReport, StringComparison.Ordinal);
            Assert.Contains("var totalActiveKits = await totalActiveKitsTask.ConfigureAwait(false);", kitReport, StringComparison.Ordinal);
            Assert.Contains("AddExactReportLimitNotice(lines, kits.Count, totalActiveKits, \"active kits\")", kitReport, StringComparison.Ordinal);
            Assert.Contains("var itemCount = FormatLimitedCount(items.Count);", kitReport, StringComparison.Ordinal);

            Assert.Contains("count >= DetailedReportResultLimit ? $\"{DetailedReportResultLimit}+\" : count.ToString();", limitNotice, StringComparison.Ordinal);
            Assert.Contains("totalCount > displayedCount", limitNotice, StringComparison.Ordinal);
            Assert.Contains("$\"Note: This report shows the first {displayedCount} of {totalCount} {recordLabel}.", limitNotice, StringComparison.Ordinal);
            Assert.Contains("Use filters or exports for a narrower full-detail review.", limitNotice, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerServiceProvidesBoundedCustomerReportSource()
        {
            var interfaceSource = ReadRepoFile("InventoryManagementApp", "Interfaces", "ICustomerService.cs");
            var customerServiceSource = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var searchMethod = ExtractMethod(
                customerServiceSource,
                "async Task<List<CustomerModel>> SearchCustomersInternalAsync(string searchTerm, CancellationToken cancellationToken)",
                "async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken)");

            Assert.Contains("Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default);", interfaceSource, StringComparison.Ordinal);
            Assert.Contains("private const int MaxCustomerSearchResults = 500;", customerServiceSource, StringComparison.Ordinal);
            Assert.Contains("ORDER BY Company ASC, Contact ASC, CustomerID ASC", searchMethod, StringComparison.Ordinal);
            Assert.Contains("LIMIT @CustomerSearchLimit", searchMethod, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@CustomerSearchLimit\", MaxCustomerSearchResults)", searchMethod, StringComparison.Ordinal);
        }

        [Fact]
        public void SummaryMaintenanceAndCalibrationCountsUseExactCountApis()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");
            var summaryReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateSummaryReport()",
                "public async Task<FlowDocument> GenerateMaintenanceReport(bool overdueOnly = false)");

            Assert.Contains("var overdueMaintenanceTask = _maintenanceService.CountOverdueMaintenanceAsync();", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var upcomingMaintenanceTask = _maintenanceService.CountUpcomingMaintenanceAsync(30);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var overdueCalibrationTask = _calibrationService.CountOverdueCalibrationAsync();", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var upcomingCalibrationTask = _calibrationService.CountUpcomingCalibrationAsync(30);", summaryReport, StringComparison.Ordinal);

            Assert.Contains("$\"Overdue Maintenance: {overdueMaintenance}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Upcoming Maintenance (30 days): {upcomingMaintenance}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Overdue Calibrations: {overdueCalibration}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Upcoming Calibrations (30 days): {upcomingCalibration}\"", summaryReport, StringComparison.Ordinal);

            Assert.DoesNotContain("var overdueMaintenanceTask = _maintenanceService.GetOverdueMaintenanceAsync();", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("var upcomingMaintenanceTask = _maintenanceService.GetUpcomingMaintenanceAsync(30);", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("var overdueCalibrationTask = _calibrationService.GetOverdueCalibrationAsync();", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("var upcomingCalibrationTask = _calibrationService.GetUpcomingCalibrationAsync(30);", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Overdue Maintenance: {FormatLimitedCount(overdueMaintenance.Count)}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Upcoming Maintenance (30 days): {FormatLimitedCount(upcomingMaintenance.Count)}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Overdue Calibrations: {FormatLimitedCount(overdueCalibration.Count)}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Upcoming Calibrations (30 days): {FormatLimitedCount(upcomingCalibration.Count)}\"", summaryReport, StringComparison.Ordinal);
        }

        [Fact]
        public void SummaryReservationAndKitCountsUseExactCountApis()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ReportService.cs");
            var summaryReport = ExtractMethod(
                source,
                "public async Task<FlowDocument> GenerateSummaryReport()",
                "public async Task<FlowDocument> GenerateMaintenanceReport(bool overdueOnly = false)");

            Assert.Contains("var activeReservationsTask = _reservationService.CountActiveReservationsAsync();", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var upcomingReservationsTask = _reservationService.CountUpcomingReservationsAsync(7);", summaryReport, StringComparison.Ordinal);
            Assert.Contains("var activeKits = await _kitService.CountActiveKitsAsync().ConfigureAwait(false);", summaryReport, StringComparison.Ordinal);

            Assert.Contains("$\"Active Reservations: {activeReservations}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Upcoming Reservations (7 days): {upcomingReservations}\"", summaryReport, StringComparison.Ordinal);
            Assert.Contains("$\"Active Kits: {activeKits}\"", summaryReport, StringComparison.Ordinal);

            Assert.DoesNotContain("var activeReservationsTask = _reservationService.GetActiveReservationsAsync();", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("var upcomingReservationsTask = _reservationService.GetUpcomingReservationsAsync(7);", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("var activeKits = await _kitService.GetActiveKitsAsync();", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Active Reservations: {FormatLimitedCount(activeReservations.Count)}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Upcoming Reservations (7 days): {FormatLimitedCount(upcomingReservations.Count)}\"", summaryReport, StringComparison.Ordinal);
            Assert.DoesNotContain("$\"Active Kits: {FormatLimitedCount(activeKits.Count)}\"", summaryReport, StringComparison.Ordinal);
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

        [Fact]
        public void MaintenanceAndCalibrationServicesProvideExactReportCounts()
        {
            var maintenanceSource = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");
            var calibrationSource = ReadRepoFile("InventoryManagementApp", "Services", "Calibration", "CalibrationService.cs");
            var maintenanceCount = ExtractMethod(
                maintenanceSource,
                "public async Task<int> CountMaintenanceRecordsAsync()",
                "public async Task<List<MaintenanceRecord>> GetMaintenanceRecordsByItemAsync(int itemID)");
            var overdueMaintenanceCount = ExtractMethod(
                maintenanceSource,
                "public async Task<int> CountOverdueMaintenanceAsync()",
                "public async Task<List<MaintenanceRecord>> GetUpcomingMaintenanceAsync(int days = 30)");
            var upcomingMaintenanceCount = ExtractMethod(
                maintenanceSource,
                "public async Task<int> CountUpcomingMaintenanceAsync(int days = 30)",
                "public async Task<MaintenanceRecord?> GetMaintenanceRecordByIdAsync(int maintenanceID)");
            var calibrationCount = ExtractMethod(
                calibrationSource,
                "public async Task<int> CountCalibrationRecordsAsync()",
                "public async Task<List<CalibrationRecord>> GetCalibrationRecordsByItemAsync(int itemID)");
            var overdueCalibrationCount = ExtractMethod(
                calibrationSource,
                "public async Task<int> CountOverdueCalibrationAsync()",
                "public async Task<List<CalibrationRecord>> GetUpcomingCalibrationAsync(int days = 30)");
            var upcomingCalibrationCount = ExtractMethod(
                calibrationSource,
                "public async Task<int> CountUpcomingCalibrationAsync(int days = 30)",
                "public async Task<CalibrationRecord?> GetLatestCalibrationForItemAsync(int itemID)");

            Assert.Contains("SELECT COUNT(m.MaintenanceID)", maintenanceCount, StringComparison.Ordinal);
            Assert.Contains("FROM MaintenanceRecords m", maintenanceCount, StringComparison.Ordinal);
            Assert.Contains("JOIN Items i ON m.ItemID = i.ItemID", maintenanceCount, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT @MaintenanceListLimit", maintenanceCount, StringComparison.Ordinal);

            Assert.Contains("SELECT COUNT(m.MaintenanceID)", overdueMaintenanceCount, StringComparison.Ordinal);
            Assert.Contains("FROM MaintenanceRecords m", overdueMaintenanceCount, StringComparison.Ordinal);
            Assert.Contains("JOIN Items i ON m.ItemID = i.ItemID", overdueMaintenanceCount, StringComparison.Ordinal);
            Assert.Contains("WHERE m.Status = 'Scheduled' AND m.ScheduledDate < @Now", overdueMaintenanceCount, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT @MaintenanceListLimit", overdueMaintenanceCount, StringComparison.Ordinal);

            Assert.Contains("SELECT COUNT(m.MaintenanceID)", upcomingMaintenanceCount, StringComparison.Ordinal);
            Assert.Contains("FROM MaintenanceRecords m", upcomingMaintenanceCount, StringComparison.Ordinal);
            Assert.Contains("JOIN Items i ON m.ItemID = i.ItemID", upcomingMaintenanceCount, StringComparison.Ordinal);
            Assert.Contains("AND m.ScheduledDate >= @Now", upcomingMaintenanceCount, StringComparison.Ordinal);
            Assert.Contains("AND m.ScheduledDate <= @FutureDate", upcomingMaintenanceCount, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT @MaintenanceListLimit", upcomingMaintenanceCount, StringComparison.Ordinal);

            Assert.Contains("SELECT COUNT(c.CalibrationID)", calibrationCount, StringComparison.Ordinal);
            Assert.Contains("FROM CalibrationRecords c", calibrationCount, StringComparison.Ordinal);
            Assert.Contains("JOIN Items i ON c.ItemID = i.ItemID", calibrationCount, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT @CalibrationListLimit", calibrationCount, StringComparison.Ordinal);

            Assert.Contains("SELECT COUNT(c.CalibrationID)", overdueCalibrationCount, StringComparison.Ordinal);
            Assert.Contains("FROM CalibrationRecords c", overdueCalibrationCount, StringComparison.Ordinal);
            Assert.Contains("JOIN Items i ON c.ItemID = i.ItemID", overdueCalibrationCount, StringComparison.Ordinal);
            Assert.Contains("WHERE c.NextCalibrationDue < @Now", overdueCalibrationCount, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT @CalibrationListLimit", overdueCalibrationCount, StringComparison.Ordinal);

            Assert.Contains("SELECT COUNT(c.CalibrationID)", upcomingCalibrationCount, StringComparison.Ordinal);
            Assert.Contains("FROM CalibrationRecords c", upcomingCalibrationCount, StringComparison.Ordinal);
            Assert.Contains("JOIN Items i ON c.ItemID = i.ItemID", upcomingCalibrationCount, StringComparison.Ordinal);
            Assert.Contains("WHERE c.NextCalibrationDue >= @Now", upcomingCalibrationCount, StringComparison.Ordinal);
            Assert.Contains("AND c.NextCalibrationDue <= @FutureDate", upcomingCalibrationCount, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT @CalibrationListLimit", upcomingCalibrationCount, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationAndKitServicesProvideExactReportCounts()
        {
            var reservationSource = ReadRepoFile("InventoryManagementApp", "Services", "Reservations", "ReservationService.cs");
            var kitSource = ReadRepoFile("InventoryManagementApp", "Services", "Kits", "KitService.cs");
            var reservationCount = ExtractMethod(
                reservationSource,
                "public async Task<int> CountReservationsAsync()",
                "public async Task<List<Reservation>> GetActiveReservationsAsync()");
            var activeReservationCount = ExtractMethod(
                reservationSource,
                "public async Task<int> CountActiveReservationsAsync()",
                "public async Task<int> CountUpcomingReservationsAsync(int days = 7)");
            var upcomingReservationCount = ExtractMethod(
                reservationSource,
                "public async Task<int> CountUpcomingReservationsAsync(int days = 7)",
                "public async Task<Reservation?> GetReservationByIdAsync(int reservationID)");
            var activeKitCount = ExtractMethod(
                kitSource,
                "public async Task<int> CountActiveKitsAsync()",
                "public async Task<Kit?> GetKitByIdAsync(int kitID)");

            Assert.Contains("SELECT COUNT(r.ReservationID)", reservationCount, StringComparison.Ordinal);
            Assert.Contains("FROM Reservations r", reservationCount, StringComparison.Ordinal);
            Assert.Contains("JOIN Items i ON r.ItemID = i.ItemID", reservationCount, StringComparison.Ordinal);
            Assert.Contains("JOIN Customers c ON r.CustomerID = c.CustomerID", reservationCount, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT @ReservationListLimit", reservationCount, StringComparison.Ordinal);

            Assert.Contains("SELECT COUNT(r.ReservationID)", activeReservationCount, StringComparison.Ordinal);
            Assert.Contains("FROM Reservations r", activeReservationCount, StringComparison.Ordinal);
            Assert.Contains("JOIN Items i ON r.ItemID = i.ItemID", activeReservationCount, StringComparison.Ordinal);
            Assert.Contains("JOIN Customers c ON r.CustomerID = c.CustomerID", activeReservationCount, StringComparison.Ordinal);
            Assert.Contains("WHERE r.Status IN ('Pending', 'Confirmed')", activeReservationCount, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT @ReservationListLimit", activeReservationCount, StringComparison.Ordinal);

            Assert.Contains("if (days < 0)", upcomingReservationCount, StringComparison.Ordinal);
            Assert.Contains("SELECT COUNT(r.ReservationID)", upcomingReservationCount, StringComparison.Ordinal);
            Assert.Contains("FROM Reservations r", upcomingReservationCount, StringComparison.Ordinal);
            Assert.Contains("JOIN Items i ON r.ItemID = i.ItemID", upcomingReservationCount, StringComparison.Ordinal);
            Assert.Contains("JOIN Customers c ON r.CustomerID = c.CustomerID", upcomingReservationCount, StringComparison.Ordinal);
            Assert.Contains("AND r.StartDate <= @FutureDate", upcomingReservationCount, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT @ReservationListLimit", upcomingReservationCount, StringComparison.Ordinal);

            Assert.Contains("SELECT COUNT(KitID)", activeKitCount, StringComparison.Ordinal);
            Assert.Contains("FROM Kits", activeKitCount, StringComparison.Ordinal);
            Assert.Contains("WHERE IsActive = 1", activeKitCount, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT @KitListLimit", activeKitCount, StringComparison.Ordinal);
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
