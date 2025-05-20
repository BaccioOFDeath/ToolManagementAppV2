using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Users;

namespace ToolManagementAppV2.Services.Tools
{
    public class ReportService
    {
        readonly ToolService _toolService;
        readonly RentalService _rentalService;
        readonly ActivityLogService _activityLogService;
        readonly CustomerService _customerService;
        readonly UserService _userService;

        public ReportService(
            ToolService toolService,
            RentalService rentalService,
            ActivityLogService activityLogService,
            CustomerService customerService,
            UserService userService)
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

        public FlowDocument GenerateActivityLogReport()
        {
            var lines = _activityLogService.GetRecentLogs(100)
                .Select(l =>
                    $"LogID: {l.LogID} | UserID: {l.UserID} | User: {l.UserName} | " +
                    $"Action: {l.Action} | Timestamp: {l.Timestamp:yyyy-MM-dd HH:mm:ss}");
            return BuildReport("Activity Log Report", lines);
        }

        public FlowDocument GenerateCustomerReport()
        {
            var lines = _customerService.GetAllCustomers()
                .Select(c =>
                    $"CustomerID: {c.CustomerID} | ToolNumber: {c.Name} | Email: {c.Email} | " +
                    $"Contact: {c.Contact} | Phone: {c.Phone} | Address: {c.Address}");
            return BuildReport("Customer Report", lines);
        }

        public FlowDocument GenerateUserReport()
        {
            var lines = _userService.GetAllUsers()
                .Select(u =>
                    $"UserID: {u.UserID} | ToolNumber: {u.UserName} | IsAdmin: {u.IsAdmin}");
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

        FlowDocument BuildReport(string title, IEnumerable<string> lines)
        {
            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
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
