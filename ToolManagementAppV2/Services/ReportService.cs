// ReportService.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2.Services
{
    public class ReportService
    {
        private readonly ToolService _toolService;
        private readonly RentalService _rentalService;
        private readonly ActivityLogService _activityLogService;
        private readonly CustomerService _customerService;
        private readonly UserService _userService;

        public ReportService(ToolService toolService, RentalService rentalService, ActivityLogService activityLogService, CustomerService customerService, UserService userService)
        {
            _toolService = toolService;
            _rentalService = rentalService;
            _activityLogService = activityLogService;
            _customerService = customerService;
            _userService = userService;
        }

        public FlowDocument GenerateInventoryReport()
        {
            var tools = _toolService.GetAllTools();
            FlowDocument doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                PageWidth = 800
            };

            Paragraph header = new Paragraph(new Run("Tool Inventory Report"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            doc.Blocks.Add(header);

            foreach (var tool in tools)
            {
                Paragraph p = new Paragraph(new Run(
                    $"Tool ID: {tool.ToolID} | Name: {tool.Name} | Qty: {tool.QuantityOnHand} | Location: {tool.Location} | Supplier: {tool.Supplier}"))
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };
                doc.Blocks.Add(p);
            }
            return doc;
        }

        public FlowDocument GenerateRentalReport(bool activeOnly = true)
        {
            // Use active rentals or full history based on parameter.
            var rentals = activeOnly ? _rentalService.GetActiveRentals() : _rentalService.GetAllRentals();
            FlowDocument doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                PageWidth = 800
            };

            string reportTitle = activeOnly ? "Active Rental Report" : "Full Rental History Report";
            Paragraph header = new Paragraph(new Run(reportTitle))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            doc.Blocks.Add(header);

            foreach (var rental in rentals)
            {
                Paragraph p = new Paragraph(new Run(
                    $"Rental ID: {rental.RentalID} | Tool ID: {rental.ToolID} | Customer ID: {rental.CustomerID} | " +
                    $"Rental Date: {rental.RentalDate:yyyy-MM-dd} | Due Date: {rental.DueDate:yyyy-MM-dd} | " +
                    $"Return Date: {(rental.ReturnDate.HasValue ? rental.ReturnDate.Value.ToString("yyyy-MM-dd") : "N/A")} | " +
                    $"Status: {rental.Status}"))
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };
                doc.Blocks.Add(p);
            }
            return doc;
        }

        public FlowDocument GenerateActivityLogReport()
        {
            var logs = _activityLogService.GetRecentLogs(100);
            FlowDocument doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                PageWidth = 800
            };

            Paragraph header = new Paragraph(new Run("Activity Log Report"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            doc.Blocks.Add(header);

            foreach (var log in logs)
            {
                Paragraph p = new Paragraph(new Run(
                    $"LogID: {log.LogID} | UserID: {log.UserID} | User: {log.UserName} | " +
                    $"Action: {log.Action} | Timestamp: {log.Timestamp:yyyy-MM-dd HH:mm:ss}"))
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };
                doc.Blocks.Add(p);
            }
            return doc;
        }

        public FlowDocument GenerateCustomerReport()
        {
            var customers = _customerService.GetAllCustomers();
            FlowDocument doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                PageWidth = 800
            };

            Paragraph header = new Paragraph(new Run("Customer Report"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            doc.Blocks.Add(header);

            foreach (var customer in customers)
            {
                Paragraph p = new Paragraph(new Run(
                    $"CustomerID: {customer.CustomerID} | Name: {customer.Name} | Email: {customer.Email} | " +
                    $"Contact: {customer.Contact} | Phone: {customer.Phone} | Address: {customer.Address}"))
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };
                doc.Blocks.Add(p);
            }
            return doc;
        }

        public FlowDocument GenerateUserReport()
        {
            var users = _userService.GetAllUsers();
            FlowDocument doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                PageWidth = 800
            };

            Paragraph header = new Paragraph(new Run("User Report"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            doc.Blocks.Add(header);

            foreach (var user in users)
            {
                Paragraph p = new Paragraph(new Run(
                    $"UserID: {user.UserID} | Name: {user.UserName} | IsAdmin: {user.IsAdmin}"))
                {
                    Margin = new Thickness(0, 0, 0, 10)
                };
                doc.Blocks.Add(p);
            }
            return doc;
        }

        public FlowDocument GenerateSummaryReport()
        {
            // Generate a report summarizing key metrics.
            var totalTools = _toolService.GetAllTools().Count;
            var totalRentals = _rentalService.GetAllRentals().Count;
            var totalActiveRentals = _rentalService.GetActiveRentals().Count;
            var totalCustomers = _customerService.GetAllCustomers().Count;
            var totalUsers = _userService.GetAllUsers().Count;

            FlowDocument doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                PageWidth = 800
            };

            Paragraph header = new Paragraph(new Run("Application Summary Report"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            doc.Blocks.Add(header);

            Paragraph summary = new Paragraph();
            summary.Inlines.Add(new Run($"Total Tools: {totalTools}\n"));
            summary.Inlines.Add(new Run($"Total Rentals (History): {totalRentals}\n"));
            summary.Inlines.Add(new Run($"Active Rentals: {totalActiveRentals}\n"));
            summary.Inlines.Add(new Run($"Total Customers: {totalCustomers}\n"));
            summary.Inlines.Add(new Run($"Total Users: {totalUsers}\n"));
            doc.Blocks.Add(summary);

            return doc;
        }
    }
}
