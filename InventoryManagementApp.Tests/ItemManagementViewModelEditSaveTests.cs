using System;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemManagementViewModelEditSaveTests
    {
        [Fact]
        public async Task EditItemCommand_WhenSaveFails_ShowsErrorAndKeepsSelectedRowStable()
        {
            var selected = new ItemModel
            {
                ItemID = 77,
                ItemNumber = "ITEM-77",
                PartNumber = "PN-77",
                Name = "Shelf impact driver",
                Brand = "Makita",
                Location = "Shelf A3",
                Price = 129.95m,
                QuantityOnHand = 2,
                RentedQuantity = 1,
                Supplier = "Main supplier",
                PurchasedDate = new DateTime(2025, 4, 12),
                Notes = "Original notes",
                Keywords = "driver cordless",
                IsPowered = true,
                IsRentalItem = true,
                IsCheckedOut = true,
                CheckedOutBy = "Jordan",
                CheckedOutTime = new DateTime(2026, 6, 16, 9, 0, 0),
                CheckedInBy = "Casey",
                CheckedInTime = new DateTime(2026, 6, 15, 17, 30, 0),
                ImagePath = "images/impact-driver.png",
                UpdatedAt = new DateTime(2026, 6, 14),
                IsIncomplete = true,
                MissingComponentsNotes = "Missing 5Ah battery",
                IssuesNotes = "Chuck needs review",
                CheckoutCount = 12
            };
            ItemModel? dialogItem = null;
            var returnedEdit = new ItemModel
            {
                ItemID = selected.ItemID,
                ItemNumber = selected.ItemNumber,
                Name = "Edited but rejected"
            };

            var itemService = new Mock<IItemService>();
            itemService
                .Setup(service => service.UpdateItemAsync(returnedEdit, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Item number already exists."));

            var dialog = new Mock<IDialogService>();
            dialog
                .Setup(service => service.ShowEditItemDialogAsync(It.IsAny<ItemModel>()))
                .Callback<ItemModel>(item => dialogItem = item)
                .ReturnsAsync(returnedEdit);
            dialog
                .Setup(service => service.ShowInfoAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var vm = new ItemManagementViewModel(
                itemService.Object,
                Mock.Of<ICustomerService>(),
                Mock.Of<IRentalService>(),
                dialog.Object,
                Mock.Of<ISettingsService>(),
                NullLogger<ItemManagementViewModel>.Instance);
            vm.Items.Add(selected);
            vm.SelectedItem = selected;

            await vm.EditItemCommand.ExecuteAsync(null);

            Assert.Same(selected, vm.SelectedItem);
            Assert.Equal("Shelf impact driver", selected.Name);
            Assert.NotNull(dialogItem);
            Assert.NotSame(selected, dialogItem);
            Assert.Equal(selected.Price, dialogItem!.Price);
            Assert.Equal(selected.UpdatedAt, dialogItem.UpdatedAt);
            Assert.Equal(selected.IsIncomplete, dialogItem.IsIncomplete);
            Assert.Equal(selected.MissingComponentsNotes, dialogItem.MissingComponentsNotes);
            Assert.Equal(selected.IssuesNotes, dialogItem.IssuesNotes);
            Assert.Equal(selected.CheckoutCount, dialogItem.CheckoutCount);
            dialog.Verify(service => service.ShowInfoAsync("Item number already exists.", "Error"), Times.Once);
        }
    }
}
