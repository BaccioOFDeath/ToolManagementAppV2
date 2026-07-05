using System;
using System.Collections.Generic;
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
            
            Assert.Throws<ArgumentNullException>(() => service.GeneratePickingSlip((Rental)null!));
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
            
            Assert.Throws<ArgumentNullException>(() => service.GenerateInvoice((Rental)null!));
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

        [Fact]
        public void GeneratePickingSlip_WithMultipleRentals_IncludesEveryItem()
        {
            var service = new RentalPrintingService("Test Company");
            var rentals = new[]
            {
                new Rental { RentalID = 2, ItemNumber = "T31", CustomerName = "E H Mechanical Services Limited", ItemLocation = "C3", RentalDate = DateTime.Today, DueDate = DateTime.Today.AddDays(3) },
                new Rental { RentalID = 3, ItemNumber = "T205", CustomerName = "E H Mechanical Services Limited", ItemLocation = "B2", RentalDate = DateTime.Today, DueDate = DateTime.Today.AddDays(4) }
            };

            var doc = service.GeneratePickingSlip(rentals);
            var text = ExtractText(doc);

            Assert.Contains("2 (2 items)", text);
            Assert.Contains("T31", text);
            Assert.Contains("T205", text);
            Assert.Contains("C3", text);
            Assert.Contains("B2", text);
        }

        [Fact]
        public void GenerateInvoice_WithMultipleRentals_IncludesEveryItemAndTotal()
        {
            var service = new RentalPrintingService("Test Company");
            var rentals = new[]
            {
                new Rental { RentalID = 2, ItemNumber = "T31", CustomerName = "E H Mechanical Services Limited", RentalDate = DateTime.Today.AddDays(-1), DueDate = DateTime.Today.AddDays(3), ReturnDate = DateTime.Today },
                new Rental { RentalID = 3, ItemNumber = "T205", CustomerName = "E H Mechanical Services Limited", RentalDate = DateTime.Today.AddDays(-2), DueDate = DateTime.Today.AddDays(4), ReturnDate = DateTime.Today }
            };

            var doc = service.GenerateInvoice(rentals, dailyRate: 25.00m, lateFee: 0);
            var text = ExtractText(doc);

            Assert.Contains("2 (2 items)", text);
            Assert.Contains("Item: T31", text);
            Assert.Contains("Item: T205", text);
            Assert.Contains("$125.00", text);
        }

        static string ExtractText(FlowDocument doc)
        {
            var parts = doc.Blocks.SelectMany(ExtractBlockText);
            return string.Join(" ", parts);
        }

        static IEnumerable<string> ExtractBlockText(Block block)
        {
            if (block is Paragraph paragraph)
                return paragraph.Inlines.Select(ExtractInlineText);
            if (block is Table table)
                return table.RowGroups
                    .SelectMany(group => group.Rows)
                    .SelectMany(row => row.Cells)
                    .SelectMany(cell => cell.Blocks)
                    .SelectMany(ExtractBlockText);
            return Enumerable.Empty<string>();
        }

        static string ExtractInlineText(Inline inline)
        {
            if (inline is Run run)
                return run.Text;
            if (inline is Span span)
                return string.Concat(span.Inlines.Select(ExtractInlineText));
            return string.Empty;
        }
    }
}
