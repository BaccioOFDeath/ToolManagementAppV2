using System;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Notifications;
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
        public void Dispose_DisposesResourcesProperly()
        {
            var service = new EmailService("smtp.test.com", 587, "user", "pass", "from@test.com", "Test");
            service.Dispose();
        }
    }
}
