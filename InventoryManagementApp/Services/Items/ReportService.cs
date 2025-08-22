using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.Generic;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Services.Items
{
    public class ReportService
    {
        readonly IItemService _itemService;
        readonly IRentalService _rentalService;
        readonly ActivityLogService _activityLogService;
        readonly ICustomerService _customerService;
        readonly IUserService _userService;

        public ReportService(
            IItemService itemService,
            IRentalService rentalService,
            ActivityLogService activityLogService,
            ICustomerService customerService,
            IUserService userService)
        {
            _itemService = itemService;
            _rentalService = rentalService;
            _activityLogService = activityLogService;
            _customerService = customerService;
            _userService = userService;
        }

        public async Task<FlowDocument> GenerateInventoryReport()
        {
            var items = new List<ItemModel>();
            await foreach (var item in _itemService.GetItemsAsync(new ItemPage(1, int.MaxValue)).ConfigureAwait(false))
                items.Add(item);
            var lines = items.Select(t =>
                $"ItemModel ID: {t.ItemID} | ItemNumber: {t.ItemNumber} | Qty: {t.QuantityOnHand} | Location: {t.Location} | Supplier: {t.Supplier}");
            return BuildReport("ItemModel Inventory Report", lines);
        }

        public async Task<FlowDocument> GenerateRentalReport(bool activeOnly = true)
        {
            var rentals = activeOnly
                ? await _rentalService.GetActiveRentalsAsync().ConfigureAwait(false)
                : await _rentalService.GetAllRentalsAsync().ConfigureAwait(false);

            var title = activeOnly ? "Active Rental Report" : "Full Rental History Report";

            var lines = rentals.Select(r =>
                $"Rental ID: {r.RentalID} | ItemModel ID: {r.ItemID} | Customer ID: {r.CustomerID} | Rental Date: {r.RentalDate:yyyy-MM-dd} | Due Date: {r.DueDate:yyyy-MM-dd} | Return Date: {(r.ReturnDate.HasValue ? r.ReturnDate.Value.ToString("yyyy-MM-dd") : "N/A")} | Status: {r.Status}");

            return BuildReport(title, lines);
        }

        public async Task<FlowDocument> GenerateActivityLogReport()
        {
            var result = await _activityLogService.GetRecentLogsAsync(100).ConfigureAwait(false);
            var logs = result?.Data ?? new List<ActivityLog>();
            var lines = logs.Select(l =>
                $"LogID: {l.LogID} | UserID: {l.UserID} | User: {l.UserName} | Action: {l.Action} | Timestamp: {l.Timestamp:yyyy-MM-dd HH:mm:ss}");
            return BuildReport("Activity Log Report", lines);
        }

        public async Task<FlowDocument> GenerateCustomerReport()
        {
            var customers = await _customerService.GetAllCustomersAsync().ConfigureAwait(false);
            var lines = customers.Select(c =>
                $"CustomerID: {c.CustomerID} | Company: {c.Company} | Email: {c.Email} | Contact: {c.Contact} | Phone: {c.Phone} | Mobile: {c.Mobile} | Address: {c.Address}");
            return BuildReport("Customer Report", lines);
        }

        public async Task<FlowDocument> GenerateUserReport()
        {
            var users = await _userService.GetAllUsersAsync().ConfigureAwait(false);
            var lines = users.Select(u =>
                $"UserID: {u.UserID} | UserName: {u.UserName} | IsAdmin: {u.IsAdmin}");
            return BuildReport("User Report", lines);
        }

        public async Task<FlowDocument> GenerateSummaryReport()
        {
            var totalItemsTask = CountItemsAsync();
            var totalRentalsTask = _rentalService.GetAllRentalsAsync();
            var totalActiveRentalsTask = _rentalService.GetActiveRentalsAsync();
            var totalCustomersTask = _customerService.GetAllCustomersAsync();
            var totalUsersTask = _userService.GetAllUsersAsync();

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

            var lines = new[]
            {
                $"Total Items: {totalItems}",
                $"Total Rentals (History): {totalRentals.Count}",
                $"Active Rentals: {totalActiveRentals.Count}",
                $"Total Customers: {totalCustomers.Count}",
                $"Total Users: {totalUsers.Count}"
            };

            return BuildReport("Application Summary Report", lines);
        }

        private async Task<int> CountItemsAsync()
        {
            var count = 0;
            await foreach (var _ in _itemService.GetItemsAsync(new ItemPage(1, int.MaxValue)))
                count++;
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

            foreach (var line in lines)
            {
                var p = new Paragraph(new Run(line)) { Margin = new Thickness(0, 0, 0, 10) };
                doc.Blocks.Add(p);
            }

            return doc;
        }
    }
}
