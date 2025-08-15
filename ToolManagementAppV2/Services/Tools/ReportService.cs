using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Services.Tools
{
    public class ReportService
    {
        readonly IToolService _toolService;
        readonly IRentalService _rentalService;
        readonly ActivityLogService _activityLogService;
        readonly ICustomerService _customerService;
        readonly IUserService _userService;

        public ReportService(
            IToolService toolService,
            IRentalService rentalService,
            ActivityLogService activityLogService,
            ICustomerService customerService,
            IUserService userService)
        {
            _toolService = toolService;
            _rentalService = rentalService;
            _activityLogService = activityLogService;
            _customerService = customerService;
            _userService = userService;
        }

        public FlowDocument GenerateInventoryReport()
        {
            var lines = _toolService.GetAllTools()
                .Select(t =>
                    $"Tool ID: {t.ToolID} | ToolNumber: {t.ToolNumber} | Qty: {t.QuantityOnHand} | " +
                    $"Location: {t.Location} | Supplier: {t.Supplier}");
            return BuildReport("Tool Inventory Report", lines);
        }

        public async Task<FlowDocument> GenerateInventoryReportAsync()
        {
            var tools = await _toolService.GetAllToolsAsync();
            var lines = tools
                .Select(t =>
                    $"Tool ID: {t.ToolID} | ToolNumber: {t.ToolNumber} | Qty: {t.QuantityOnHand} | " +
                    $"Location: {t.Location} | Supplier: {t.Supplier}");
            return BuildReport("Tool Inventory Report", lines);
        }

        public FlowDocument GenerateRentalReport(bool activeOnly = true)
        {
            var rentals = activeOnly
                ? _rentalService.GetActiveRentals()
                : _rentalService.GetAllRentals();

            var title = activeOnly
                ? "Active Rental Report"
                : "Full Rental History Report";

            var lines = rentals.Select(r =>
                $"Rental ID: {r.RentalID} | Tool ID: {r.ToolID} | Customer ID: {r.CustomerID} | " +
                $"Rental Date: {r.RentalDate:yyyy-MM-dd} | Due Date: {r.DueDate:yyyy-MM-dd} | " +
                $"Return Date: {(r.ReturnDate.HasValue ? r.ReturnDate.Value.ToString("yyyy-MM-dd") : "N/A")} | " +
                $"Status: {r.Status}");

            return BuildReport(title, lines);
        }

        public async Task<FlowDocument> GenerateRentalReportAsync(bool activeOnly = true)
        {
            var rentals = activeOnly
                ? await _rentalService.GetActiveRentalsAsync()
                : await _rentalService.GetAllRentalsAsync();

            var title = activeOnly
                ? "Active Rental Report"
                : "Full Rental History Report";

            var lines = rentals.Select(r =>
                $"Rental ID: {r.RentalID} | Tool ID: {r.ToolID} | Customer ID: {r.CustomerID} | " +
                $"Rental Date: {r.RentalDate:yyyy-MM-dd} | Due Date: {r.DueDate:yyyy-MM-dd} | " +
                $"Return Date: {(r.ReturnDate.HasValue ? r.ReturnDate.Value.ToString("yyyy-MM-dd") : "N/A")} | " +
                $"Status: {r.Status}");

            return BuildReport(title, lines);
        }

        public FlowDocument GenerateActivityLogReport()
        {
            var logs = _activityLogService.GetRecentLogs(100) ?? new List<ActivityLog>();
            var lines = logs.Select(l =>
                    $"LogID: {l.LogID} | UserID: {l.UserID} | User: {l.UserName} | " +
                    $"Action: {l.Action} | Timestamp: {l.Timestamp:yyyy-MM-dd HH:mm:ss}");
            return BuildReport("Activity Log Report", lines);
        }

        public async Task<FlowDocument> GenerateActivityLogReportAsync()
        {
            var logs = await _activityLogService.GetRecentLogsAsync(100) ?? new List<ActivityLog>();
            var lines = logs.Select(l =>
                    $"LogID: {l.LogID} | UserID: {l.UserID} | User: {l.UserName} | " +
                    $"Action: {l.Action} | Timestamp: {l.Timestamp:yyyy-MM-dd HH:mm:ss}");
            return BuildReport("Activity Log Report", lines);
        }

        public FlowDocument GenerateCustomerReport()
        {
            var lines = _customerService.GetAllCustomers()
                .Select(c =>
                    $"CustomerID: {c.CustomerID} | Company: {c.Company} | Email: {c.Email} | " +
                    $"Contact: {c.Contact} | Phone: {c.Phone} | Mobile: {c.Mobile} | Address: {c.Address}");
            return BuildReport("Customer Report", lines);
        }

        public async Task<FlowDocument> GenerateCustomerReportAsync()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var lines = customers
                .Select(c =>
                    $"CustomerID: {c.CustomerID} | Company: {c.Company} | Email: {c.Email} | " +
                    $"Contact: {c.Contact} | Phone: {c.Phone} | Mobile: {c.Mobile} | Address: {c.Address}");
            return BuildReport("Customer Report", lines);
        }

        public FlowDocument GenerateUserReport()
        {
            var lines = _userService.GetAllUsers()
                .Select(u =>
                    $"UserID: {u.UserID} | UserName: {u.UserName} | IsAdmin: {u.IsAdmin}");
            return BuildReport("User Report", lines);
        }

        public async Task<FlowDocument> GenerateUserReportAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            var lines = users
                .Select(u =>
                    $"UserID: {u.UserID} | UserName: {u.UserName} | IsAdmin: {u.IsAdmin}");
            return BuildReport("User Report", lines);
        }

        public FlowDocument GenerateSummaryReport()
        {
            var totalTools = _toolService.GetAllTools().Count;
            var totalRentals = _rentalService.GetAllRentals().Count;
            var totalActiveRentals = _rentalService.GetActiveRentals().Count;
            var totalCustomers = _customerService.GetAllCustomers().Count;
            var totalUsers = _userService.GetAllUsers().Count;

            var lines = new[]
            {
                $"Total Tools: {totalTools}",
                $"Total Rentals (History): {totalRentals}",
                $"Active Rentals: {totalActiveRentals}",
                $"Total Customers: {totalCustomers}",
                $"Total Users: {totalUsers}"
            };

            return BuildReport("Application Summary Report", lines);
        }

        public async Task<FlowDocument> GenerateSummaryReportAsync()
        {
            var totalToolsTask = _toolService.GetAllToolsAsync();
            var totalRentalsTask = _rentalService.GetAllRentalsAsync();
            var totalActiveRentalsTask = _rentalService.GetActiveRentalsAsync();
            var totalCustomersTask = _customerService.GetAllCustomersAsync();
            var totalUsersTask = _userService.GetAllUsersAsync();

            await Task.WhenAll(totalToolsTask, totalRentalsTask, totalActiveRentalsTask, totalCustomersTask, totalUsersTask);

            var lines = new[]
            {
                $"Total Tools: {totalToolsTask.Result.Count}",
                $"Total Rentals (History): {totalRentalsTask.Result.Count}",
                $"Active Rentals: {totalActiveRentalsTask.Result.Count}",
                $"Total Customers: {totalCustomersTask.Result.Count}",
                $"Total Users: {totalUsersTask.Result.Count}"
            };

            return BuildReport("Application Summary Report", lines);
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
