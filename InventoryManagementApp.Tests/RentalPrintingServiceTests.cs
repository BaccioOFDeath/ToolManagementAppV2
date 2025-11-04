using System;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Printing;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalPrintingServiceTests
    {
        [Fact]
        public void GeneratePickingSlip_WithNullRental_ThrowsArgumentNullException()
        {
            var service = new RentalPrintingService();
            
            Assert.Throws<ArgumentNullException>(() => service.GeneratePickingSlip(null!));
        }

        [Fact]
        public void GeneratePickingSlip_WithValidRental_ReturnsFlowDocument()
        {
            var service = new RentalPrintingService("Test Company", "123 Test St", "(555) 123-4567");
            var rental = new Rental
            {
                RentalID = 1,
                ItemNumber = "ITEM001",
                CustomerName = "Test Customer",
                CustomerContact = "John Doe",
                CustomerEmail = "john@test.com",
                CustomerPhone = "(555) 987-6543",
                ItemLocation = "Warehouse A",
                RentalDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(7)
            };

            var doc = service.GeneratePickingSlip(rental);

            Assert.NotNull(doc);
            Assert.NotEmpty(doc.Blocks);
        }

        [Fact]
        public void GenerateInvoice_WithNullRental_ThrowsArgumentNullException()
        {
            var service = new RentalPrintingService();
            
            Assert.Throws<ArgumentNullException>(() => service.GenerateInvoice(null!));
        }

        [Fact]
        public void GenerateInvoice_WithValidRental_ReturnsFlowDocument()
        {
            var service = new RentalPrintingService("Test Company", "123 Test St", "(555) 123-4567");
            var rental = new Rental
            {
                RentalID = 1,
                ItemNumber = "ITEM001",
                CustomerName = "Test Customer",
                CustomerContact = "John Doe",
                CustomerEmail = "john@test.com",
                CustomerAddress = "456 Main St",
                RentalDate = DateTime.Today.AddDays(-5),
                DueDate = DateTime.Today.AddDays(2),
                ReturnDate = DateTime.Today
            };

            var doc = service.GenerateInvoice(rental, dailyRate: 25.00m, lateFee: 0);

            Assert.NotNull(doc);
            Assert.NotEmpty(doc.Blocks);
        }

        [Fact]
        public void GenerateInvoice_WithLateFee_IncludesLateFeeInDocument()
        {
            var service = new RentalPrintingService("Test Company");
            var rental = new Rental
            {
                RentalID = 1,
                ItemNumber = "ITEM001",
                CustomerName = "Test Customer",
                RentalDate = DateTime.Today.AddDays(-10),
                DueDate = DateTime.Today.AddDays(-3),
                ReturnDate = DateTime.Today
            };

            var doc = service.GenerateInvoice(rental, dailyRate: 25.00m, lateFee: 50.00m);

            Assert.NotNull(doc);
            Assert.NotEmpty(doc.Blocks);
        }
    }
}
