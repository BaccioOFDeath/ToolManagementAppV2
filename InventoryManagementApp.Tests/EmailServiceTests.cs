using System;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Notifications;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class EmailServiceTests
    {
        [Fact]
        public void Constructor_WithNullSmtpHost_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EmailService(null!, 587, "user", "pass", "from@test.com", "Test"));
        }

        [Fact]
        public void Constructor_WithNullUsername_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EmailService("smtp.test.com", 587, null!, "pass", "from@test.com", "Test"));
        }

        [Fact]
        public void Constructor_WithNullPassword_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EmailService("smtp.test.com", 587, "user", null!, "from@test.com", "Test"));
        }

        [Fact]
        public void Constructor_WithNullFromEmail_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new EmailService("smtp.test.com", 587, "user", "pass", null!, "Test"));
        }

        [Fact]
        public async Task SendRentalReminderAsync_WithEmptyEmail_LogsWarningAndReturns()
        {
            var service = new EmailService("smtp.test.com", 587, "user", "pass", "from@test.com", "Test");
            
            await service.SendRentalReminderAsync("", "Customer", "ITEM001", DateTime.Today, "Contact Info");
        }

        [Fact]
        public async Task SendEmailAsync_WithEmptyEmail_LogsWarningAndReturns()
        {
            var service = new EmailService("smtp.test.com", 587, "user", "pass", "from@test.com", "Test");
            
            await service.SendEmailAsync("", "Subject", "Body");
        }

        [Fact]
        public void BuildOverdueRentalPreviewBody_IncludesCustomerItemDueDateAndContact()
        {
            var dueDate = DateTime.Today.AddDays(-3);

            var body = SettingsViewModel.BuildOverdueRentalPreviewBody("Sample Customer", "TL-318", dueDate, "rentals@example.com");

            Assert.Contains("Dear Sample Customer", body, StringComparison.Ordinal);
            Assert.Contains("Item Number: TL-318", body, StringComparison.Ordinal);
            Assert.Contains($"Due Date: {dueDate:yyyy-MM-dd}", body, StringComparison.Ordinal);
            Assert.Contains("Days Overdue: 3", body, StringComparison.Ordinal);
            Assert.Contains("rentals@example.com", body, StringComparison.Ordinal);
        }

        [Fact]
        public void BuildBrandedHtml_UsesCampaignLayoutForRentalItemsWithoutLogo()
        {
            var html = EmailService.BuildBrandedHtml(
                "Reminder: Item TL-101 Due Tomorrow",
                "Dear Customer,\n\nItem Number: TL-101\nDue Date: 2026-06-30",
                "SD European",
                "Regards,\nRental Team",
                "company-logo",
                "item-image");

            Assert.DoesNotContain("cid:company-logo", html, StringComparison.Ordinal);
            Assert.Contains("cid:item-image", html, StringComparison.Ordinal);
            Assert.Contains("SD European", html, StringComparison.Ordinal);
            Assert.Contains("Rental Team", html, StringComparison.Ordinal);
            Assert.Contains("Rental item notice", html, StringComparison.Ordinal);
            Assert.Contains("Contact the rental team", html, StringComparison.Ordinal);
            Assert.Contains("background:#0f0f0f", html, StringComparison.Ordinal);
            Assert.Contains("background:#f5b700", html, StringComparison.Ordinal);
            Assert.Contains("border-radius:16px", html, StringComparison.Ordinal);
            Assert.Contains("width=\"600\"", html, StringComparison.Ordinal);
            Assert.Contains("style=\"width:600px;max-width:600px", html, StringComparison.Ordinal);
            Assert.Contains("max-width:540px", html, StringComparison.Ordinal);
            Assert.Contains("max-width:180px", html, StringComparison.Ordinal);
            Assert.Contains("max-height:180px", html, StringComparison.Ordinal);
            Assert.Contains("<td style=\"width:38%;padding:13px 16px;background:#f7f8fa;border-top:0;color:#6b7280;font-size:12px;font-weight:700;\">Item Number</td>", html, StringComparison.Ordinal);
            Assert.Contains("<td style=\"padding:13px 16px;border-top:0;color:#1c1c1e;font-size:15px;font-weight:800;\">TL-101</td>", html, StringComparison.Ordinal);
            Assert.Contains("<td style=\"width:38%;padding:13px 16px;background:#f7f8fa;border-top:1px solid #e2e4e8;color:#6b7280;font-size:12px;font-weight:700;\">Due Date</td>", html, StringComparison.Ordinal);
            Assert.Contains("<td style=\"padding:13px 16px;border-top:1px solid #e2e4e8;color:#1c1c1e;font-size:15px;font-weight:800;\">2026-06-30</td>", html, StringComparison.Ordinal);
            Assert.DoesNotContain("WOF", html, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void BuildBrandedHtml_UsesReturnRequiredTreatmentForOverdueItems()
        {
            var html = EmailService.BuildBrandedHtml(
                "Overdue: Item TL-318",
                "Dear Customer,\n\nItem Number: TL-318\nDue Date: 2026-06-26\nDays Overdue: 3",
                "SD European",
                null,
                null,
                null);

            Assert.Contains("Return required", html, StringComparison.Ordinal);
            Assert.Contains("Please return the item or contact the rental team to arrange an extension.", html, StringComparison.Ordinal);
            Assert.Contains("#b91c1c", html, StringComparison.Ordinal);
            Assert.Contains("Days Overdue", html, StringComparison.Ordinal);
            Assert.Contains(">3</td>", html, StringComparison.Ordinal);
        }

        [Fact]
        public void Dispose_DisposesResourcesProperly()
        {
            var service = new EmailService("smtp.test.com", 587, "user", "pass", "from@test.com", "Test");
            service.Dispose();
        }
    }
}
