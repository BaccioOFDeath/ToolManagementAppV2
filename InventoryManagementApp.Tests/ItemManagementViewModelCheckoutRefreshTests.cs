using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
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

        [Fact]
        public async Task ToggleCheckOutCommand_RefreshesCheckedOutListWhenSearchIsActive()
        {
            var searchRow = new ItemModel
            {
                ItemID = 99,
                ItemNumber = "T99",
                Name = "Search result row",
                IsCheckedOut = false
            };
            var refreshed = new ItemModel
            {
                ItemID = 99,
                ItemNumber = "T99",
                Name = "Checked out from storage",
                IsCheckedOut = true,
                CheckedOutBy = "Alex",
                CheckedOutTime = new DateTime(2026, 6, 18, 9, 0, 0)
            };

            var itemService = new Mock<IItemService>();
            itemService
                .Setup(s => s.ToggleItemCheckOutStatusAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            itemService
                .Setup(s => s.GetItemByIDAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshed);
            itemService
                .Setup(s => s.GetCheckedOutItemsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemModel> { refreshed });

            var vm = new ItemManagementViewModel(
                itemService.Object,
                Mock.Of<ICustomerService>(),
                Mock.Of<IRentalService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<ISettingsService>(),
                NullLogger<ItemManagementViewModel>.Instance);
            vm.SearchResults.Add(searchRow);

            await vm.ToggleCheckOutCommand.ExecuteAsync(searchRow);

            var checkedOutRow = Assert.Single(vm.CheckedOutItems);
            Assert.Equal(99, checkedOutRow.ItemID);
            Assert.Equal("Checked out from storage", checkedOutRow.Name);
            Assert.True(searchRow.IsCheckedOut);
        }

        [Fact]
        public async Task SearchCommand_RefreshesCheckedOutListWhenSearchTermIsActive()
        {
            var searchResult = new ItemModel
            {
                ItemID = 10,
                ItemNumber = "T10",
                Name = "Search match",
                IsCheckedOut = true
            };
            var checkedOut = new ItemModel
            {
                ItemID = 10,
                ItemNumber = "T10",
                Name = "Checked out match",
                IsCheckedOut = true,
                CheckedOutBy = "admin",
                CheckedOutTime = new DateTime(2026, 6, 21, 21, 52, 0)
            };

            var itemService = new Mock<IItemService>();
            itemService
                .Setup(s => s.SearchItemsAsync(
                    "bmw m52",
                    It.IsAny<ItemPage>(),
                    SortField.Name,
                    SortDirection.Ascending,
                    null,
                    It.IsAny<CancellationToken>()))
                .Returns(ToAsyncEnumerable(new[] { searchResult }));
            itemService
                .Setup(s => s.GetCheckedOutItemsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemModel> { checkedOut });

            var vm = new ItemManagementViewModel(
                itemService.Object,
                Mock.Of<ICustomerService>(),
                Mock.Of<IRentalService>(),
                Mock.Of<IDialogService>(),
                Mock.Of<ISettingsService>(),
                NullLogger<ItemManagementViewModel>.Instance)
            {
                SearchTerm = "bmw m52"
            };

            await vm.SearchCommand.ExecuteAsync(null);

            Assert.Single(vm.SearchResults);
            var checkedOutRow = Assert.Single(vm.CheckedOutItems);
            Assert.Equal("T10", checkedOutRow.ItemNumber);
            Assert.Equal("admin", checkedOutRow.CheckedOutBy);
        }

        private static async IAsyncEnumerable<ItemModel> ToAsyncEnumerable(IEnumerable<ItemModel> items)
        {
            foreach (var item in items)
            {
                await Task.Yield();
                yield return item;
            }
        }
    }
}
