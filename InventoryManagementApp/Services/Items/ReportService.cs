using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services.Maintenance;
using InventoryManagementApp.Services.Calibration;
using InventoryManagementApp.Services.Reservations;
using InventoryManagementApp.Services.Kits;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Services.Items
{
    public class ReportService
    {
        private const int InventoryReportPageSize = 500;

        readonly IItemService _itemService;
        readonly IRentalService _rentalService;
        readonly ActivityLogService _activityLogService;
        readonly ICustomerService _customerService;
        readonly IUserService _userService;
        readonly MaintenanceService? _maintenanceService;
        readonly CalibrationService? _calibrationService;
        readonly ReservationService? _reservationService;
        readonly KitService? _kitService;

        public ReportService(
            IItemService itemService,
            IRentalService rentalService,
            ActivityLogService activityLogService,
            ICustomerService customerService,
            IUserService userService,
            MaintenanceService? maintenanceService = null,
            CalibrationService? calibrationService = null,
            ReservationService? reservationService = null,
            KitService? kitService = null)
        {
            _itemService = itemService;
            _rentalService = rentalService;
            _activityLogService = activityLogService;
            _customerService = customerService;
            _userService = userService;
            _maintenanceService = maintenanceService;
            _calibrationService = calibrationService;
            _reservationService = reservationService;
            _kitService = kitService;
        }

        public async Task<FlowDocument> GenerateInventoryReport()
        {
            var items = await CollectInventoryReportItemsAsync().ConfigureAwait(false);
            var lines = items.Select(t =>
                $"Item ID: {t.ItemID} | Item Number: {t.ItemNumber} | Quantity: {t.QuantityOnHand} | Location: {t.Location} | Supplier: {t.Supplier}");
            return BuildReport("Inventory Report", lines);
        }

        public async Task<FlowDocument> GenerateRentalReport(bool activeOnly = true)
        {
            var rentals = activeOnly
                ? await _rentalService.GetActiveRentalsAsync().ConfigureAwait(false)
                : await _rentalService.GetAllRentalsAsync().ConfigureAwait(false);

            var title = activeOnly ? "Active Rental Report" : "Full Rental History Report";

            var lines = rentals.Select(r =>
                $"Rental ID: {r.RentalID} | Item ID: {r.ItemID} | Customer ID: {r.CustomerID} | Rental Date: {r.RentalDate:yyyy-MM-dd} | Due Date: {r.DueDate:yyyy-MM-dd} | Return Date: {(r.ReturnDate.HasValue ? r.ReturnDate.Value.ToString("yyyy-MM-dd") : "N/A")} | Status: {r.Status}");

            return BuildReport(title, lines);
        }

        public async Task<FlowDocument> GenerateRentalFrequencyReport(int topN = 20)
        {
            var frequencies = await _rentalService.GetRentalFrequencyAsync(topN).ConfigureAwait(false);
            var lines = frequencies.Select(f =>
                $"Item Number: {f.ItemNumber} | Name: {f.ItemName} | Times Rented: {f.RentalCount}");
            return BuildReport($"Most Frequently Rented Items (Top {topN})", lines);
        }

        public async Task<FlowDocument> GenerateCommonlyUsedItemsReport(int topN = 25)
        {
            var items = await _itemService.GetMostCommonlyUsedItemsAsync(topN, CancellationToken.None).ConfigureAwait(false);
            var lines = items.Select(item =>
                $"Item Number: {item.ItemNumber} | Name: {item.Name} | Location: {item.Location} | Use Count: {item.CheckoutCount} | Status: {item.AvailabilityStatus}");
            return BuildReport($"Commonly Used Items Report (Top {topN})", lines);
        }


        public async Task<FlowDocument> GenerateActivityLogReport()
        {
            var result = await _activityLogService.GetRecentLogsAsync(100).ConfigureAwait(false);
            var logs = result?.Success == true && result.Value != null
                ? result.Value
                : new List<ActivityLog>();
            var lines = logs.Select(l =>
                $"Log ID: {l.LogID} | User ID: {l.UserID} | User: {l.UserName} | Action: {l.Action} | Timestamp: {l.Timestamp:yyyy-MM-dd HH:mm:ss}");
            return BuildReport("Activity Log Report", lines);
        }

        public async Task<FlowDocument> GenerateCustomerReport()
        {
            var customers = await _customerService.GetAllCustomersAsync().ConfigureAwait(false);
            var lines = customers.Select(c =>
                $"Customer ID: {c.CustomerID} | Company: {c.Company} | Email: {c.Email} | Contact: {c.Contact} | Phone: {c.Phone} | Mobile: {c.Mobile} | Address: {c.Address}");
            return BuildReport("Customer Report", lines);
        }

        public async Task<FlowDocument> GenerateUserReport()
        {
            var users = await _userService.GetAllUsersAsync(CancellationToken.None).ConfigureAwait(false);
            var lines = users.Select(u =>
                $"User ID: {u.UserID} | User Name: {u.UserName} | Admin: {u.IsAdmin}");
            return BuildReport("User Report", lines);
        }

        public async Task<FlowDocument> GenerateSummaryReport()
        {
            var totalItemsTask = CountItemsAsync();
            var totalRentalsTask = _rentalService.GetAllRentalsAsync();
            var totalActiveRentalsTask = _rentalService.GetActiveRentalsAsync();
            var totalCustomersTask = _customerService.GetAllCustomersAsync();
            var totalUsersTask = _userService.GetAllUsersAsync(CancellationToken.None);

            await Task.WhenAll(
                totalItemsTask,
                totalRentalsTask,
                totalActiveRentalsTask,
                totalCustomersTask,
                totalUsersTask).ConfigureAwait(false);

            var totalItems = await totalItemsTask.ConfigureAwait(false);
            var totalRentals = await totalRentalsTask.ConfigureAwait(false);
            var totalActiveRentals = await totalActiveRentalsTask.ConfigureAwait(false);
            var totalCustomers = await totalCustomersTask.ConfigureAwait(false);
            var totalUsers = await totalUsersTask.ConfigureAwait(false);

            var lines = new List<string>
            {
                $"Total Items: {totalItems}",
                $"Total Rentals (History): {totalRentals.Count}",
                $"Active Rentals: {totalActiveRentals.Count}",
                $"Total Customers: {totalCustomers.Count}",
                $"Total Users: {totalUsers.Count}"
            };

            if (_maintenanceService != null)
            {
                var overdueMaintenanceTask = _maintenanceService.GetOverdueMaintenanceAsync();
                var upcomingMaintenanceTask = _maintenanceService.GetUpcomingMaintenanceAsync(30);
                await Task.WhenAll(overdueMaintenanceTask, upcomingMaintenanceTask);
                var overdueMaintenance = await overdueMaintenanceTask;
                var upcomingMaintenance = await upcomingMaintenanceTask;
                lines.Add($"Overdue Maintenance: {overdueMaintenance.Count}");
                lines.Add($"Upcoming Maintenance (30 days): {upcomingMaintenance.Count}");
            }

            if (_calibrationService != null)
            {
                var overdueCalibrationTask = _calibrationService.GetOverdueCalibrationAsync();
                var upcomingCalibrationTask = _calibrationService.GetUpcomingCalibrationAsync(30);
                await Task.WhenAll(overdueCalibrationTask, upcomingCalibrationTask);
                var overdueCalibration = await overdueCalibrationTask;
                var upcomingCalibration = await upcomingCalibrationTask;
                lines.Add($"Overdue Calibrations: {overdueCalibration.Count}");
                lines.Add($"Upcoming Calibrations (30 days): {upcomingCalibration.Count}");
            }

            if (_reservationService != null)
            {
                var activeReservationsTask = _reservationService.GetActiveReservationsAsync();
                var upcomingReservationsTask = _reservationService.GetUpcomingReservationsAsync(7);
                await Task.WhenAll(activeReservationsTask, upcomingReservationsTask);
                var activeReservations = await activeReservationsTask;
                var upcomingReservations = await upcomingReservationsTask;
                lines.Add($"Active Reservations: {activeReservations.Count}");
                lines.Add($"Upcoming Reservations (7 days): {upcomingReservations.Count}");
            }

            if (_kitService != null)
            {
                var activeKits = await _kitService.GetActiveKitsAsync();
                lines.Add($"Active Kits: {activeKits.Count}");
            }

            return BuildReport("Application Summary Report", lines);
        }

        public async Task<FlowDocument> GenerateMaintenanceReport(bool overdueOnly = false)
        {
            if (_maintenanceService == null)
                return BuildReport("Maintenance Report", new[] { "Maintenance service not available" });

            var records = overdueOnly
                ? await _maintenanceService.GetOverdueMaintenanceAsync().ConfigureAwait(false)
                : await _maintenanceService.GetAllMaintenanceRecordsAsync().ConfigureAwait(false);

            var title = overdueOnly ? "Overdue Maintenance Report" : "Maintenance Schedule Report";

            var lines = records.Select(m =>
                $"Maintenance ID: {m.MaintenanceID} | Item: {m.ItemNumber} - {m.ItemName} | Type: {m.MaintenanceType} | Scheduled: {m.ScheduledDate:yyyy-MM-dd} | Status: {m.StatusDisplay} | Cost: ${m.Cost:F2}");

            return BuildReport(title, lines);
        }

        public async Task<FlowDocument> GenerateCalibrationReport(bool overdueOnly = false)
        {
            if (_calibrationService == null)
                return BuildReport("Calibration Report", new[] { "Calibration service not available" });

            var records = overdueOnly
                ? await _calibrationService.GetOverdueCalibrationAsync().ConfigureAwait(false)
                : await _calibrationService.GetAllCalibrationRecordsAsync().ConfigureAwait(false);

            var title = overdueOnly ? "Overdue Calibration Report" : "Calibration Records Report";

            var lines = records.Select(c =>
                $"Calibration ID: {c.CalibrationID} | Item: {c.ItemNumber} - {c.ItemName} | Date: {c.CalibrationDate:yyyy-MM-dd} | Next Due: {c.NextCalibrationDue:yyyy-MM-dd} | Status: {c.StatusDisplay} | Certificate Number: {c.CertificateNumber}");

            return BuildReport(title, lines);
        }

        public async Task<FlowDocument> GenerateReservationReport(bool activeOnly = true)
        {
            if (_reservationService == null)
                return BuildReport("Reservation Report", new[] { "Reservation service not available" });

            var reservations = activeOnly
                ? await _reservationService.GetActiveReservationsAsync().ConfigureAwait(false)
                : await _reservationService.GetAllReservationsAsync().ConfigureAwait(false);

            var title = activeOnly ? "Active Reservations Report" : "All Reservations Report";

            var lines = reservations.Select(r =>
                $"Reservation ID: {r.ReservationID} | Item: {r.ItemNumber} - {r.ItemName} | Customer: {r.CustomerName} | Start: {r.StartDate:yyyy-MM-dd} | End: {r.EndDate:yyyy-MM-dd} | Quantity: {r.Quantity} | Status: {r.StatusDisplay}");

            return BuildReport(title, lines);
        }

        public async Task<FlowDocument> GenerateKitReport()
        {
            if (_kitService == null)
                return BuildReport("Kit Report", new[] { "Kit service not available" });

            var kits = await _kitService.GetActiveKitsAsync().ConfigureAwait(false);
            var lines = new List<string>();

            foreach (var kit in kits)
            {
                var items = await _kitService.GetKitItemsAsync(kit.KitID).ConfigureAwait(false);
                var itemCount = items.Count;
                lines.Add($"Kit Number: {kit.KitNumber} | Kit Name: {kit.Name} | Category: {kit.Category} | Item Count: {itemCount} | Status: {(kit.IsActive ? "Active" : "Inactive")}");
            }

            return BuildReport("Active Kits Report", lines);
        }

        private async Task<List<ItemModel>> CollectInventoryReportItemsAsync()
        {
            var items = new List<ItemModel>();
            var pageNumber = 1;

            while (true)
            {
                var pageItemCount = 0;
                await foreach (var item in _itemService.GetItemsAsync(new ItemPage(pageNumber, InventoryReportPageSize), SortField.Name, SortDirection.Ascending, isRentalItem: false)
                    .WithCancellation(CancellationToken.None).ConfigureAwait(false))
                {
                    items.Add(item);
                    pageItemCount++;
                }

                if (pageItemCount < InventoryReportPageSize)
                    break;

                pageNumber++;
            }

            return items;
        }

        private async Task<int> CountItemsAsync()
        {
            var count = 0;
            var pageNumber = 1;

            while (true)
            {
                var pageItemCount = 0;
                await foreach (var _ in _itemService.GetItemsAsync(new ItemPage(pageNumber, InventoryReportPageSize), SortField.Name, SortDirection.Ascending, isRentalItem: false)
                    .WithCancellation(CancellationToken.None).ConfigureAwait(false))
                {
                    count++;
                    pageItemCount++;
                }

                if (pageItemCount < InventoryReportPageSize)
                    break;

                pageNumber++;
            }

            return count;
        }


        FlowDocument BuildReport(string title, IEnumerable<string> lines)
        {
            var doc = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 12,
                PageWidth = 800
            };

            var header = new Paragraph(new Run(title))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            doc.Blocks.Add(header);

            var reportLines = lines as IReadOnlyCollection<string> ?? lines.ToList();
            if (reportLines.Count == 0)
                reportLines = new[] { "No report records found." };

            foreach (var line in reportLines)
            {
                var p = new Paragraph(new Run(line)) { Margin = new Thickness(0, 0, 0, 10) };
                doc.Blocks.Add(p);
            }

            return doc;
        }
    }
}