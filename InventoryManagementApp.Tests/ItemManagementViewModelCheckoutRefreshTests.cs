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
    public class ItemManagementViewModelCheckoutRefreshTests
    {
        [Fact]
        public async Task ToggleCheckOutCommand_UsesPersistedItemStateAfterSuccessfulToggle()
        {
            var checkedOutAt = new DateTime(2026, 6, 16, 8, 30, 0);
            var refreshed = new ItemModel
            {
                ItemID = 42,
                ItemNumber = "T42",
                Name = "Persisted torque wrench",
                Location = "Rack B",
                QuantityOnHand = 5,
                IsCheckedOut = true,
                CheckedOutBy = "Alex",
                CheckedOutTime = checkedOutAt,
                CheckoutCount = 9
            };

            var itemService = new Mock<IItemService>();
            itemService
                .Setup(s => s.ToggleItemCheckOutStatusAsync(42, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            itemService
                .Setup(s => s.GetItemByIDAsync(42, It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshed);

            var vm = new ItemManagementViewModel(
                itemService.Object,
                Mock.Of<ICustomerService>(),
                Mock.Of<IRentalService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<ISettingsService>(),
                NullLogger<ItemManagementViewModel>.Instance);
            var staleRow = new ItemModel
            {
                ItemID = 42,
                ItemNumber = "OLD",
                Name = "Stale row",
                Location = "Old rack",
                QuantityOnHand = 1,
                IsCheckedOut = false,
                CheckoutCount = 0
            };
            vm.Items.Add(staleRow);

            await vm.ToggleCheckOutCommand.ExecuteAsync(staleRow);

            Assert.True(staleRow.IsCheckedOut);
            Assert.Equal("T42", staleRow.ItemNumber);
            Assert.Equal("Persisted torque wrench", staleRow.Name);
            Assert.Equal("Rack B", staleRow.Location);
            Assert.Equal(5, staleRow.QuantityOnHand);
            Assert.Equal("Alex", staleRow.CheckedOutBy);
            Assert.Equal(checkedOutAt, staleRow.CheckedOutTime);
            Assert.Equal(9, staleRow.CheckoutCount);
            var checkedOutRow = Assert.Single(vm.CheckedOutItems);
            Assert.Same(staleRow, checkedOutRow);
        }
    }
}
