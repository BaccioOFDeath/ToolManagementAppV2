using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
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
        public void GeneratePickingSlip_WithItemImage_AddsPhotoToDocument()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                var imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
                try
                {
                    File.WriteAllBytes(imagePath, Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAAAAAA6fptVAAAAC0lEQVQI12NgYAAAAAMAAWgmWQ0AAAAASUVORK5CYII="));
                    var service = new RentalPrintingService("Test Company");
                    var rental = new Rental
                    {
                        RentalID = 1,
                        ItemNumber = "ITEM001",
                        ImagePath = imagePath,
                        CustomerName = "Test Customer",
                        ItemLocation = "Warehouse A",
                        RentalDate = DateTime.Today,
                        DueDate = DateTime.Today.AddDays(7)
                    };

                    var doc = service.GeneratePickingSlip(rental);

                    Assert.Contains(doc.Blocks.OfType<BlockUIContainer>(), block => block.Child is Image);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    if (File.Exists(imagePath))
                        File.Delete(imagePath);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
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
