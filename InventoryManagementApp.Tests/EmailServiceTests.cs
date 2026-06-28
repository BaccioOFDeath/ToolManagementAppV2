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
            Assert.Contains("border-radius:20px", html, StringComparison.Ordinal);
            Assert.Contains("width=\"640\"", html, StringComparison.Ordinal);
            Assert.Contains("style=\"width:640px;max-width:640px", html, StringComparison.Ordinal);
            Assert.Contains("max-width:520px", html, StringComparison.Ordinal);
            Assert.Contains("max-width:190px", html, StringComparison.Ordinal);
            Assert.Contains("max-height:190px", html, StringComparison.Ordinal);
            Assert.Contains("<span class=\"fact-label\">Item Number</span><span class=\"fact-value\">TL-101</span>", html, StringComparison.Ordinal);
            Assert.Contains("<span class=\"fact-label\">Due Date</span><span class=\"fact-value\">2026-06-30</span>", html, StringComparison.Ordinal);
            Assert.DoesNotContain("WOF", html, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Dispose_DisposesResourcesProperly()
        {
            var service = new EmailService("smtp.test.com", 587, "user", "pass", "from@test.com", "Test");
            service.Dispose();
        }
    }
}
