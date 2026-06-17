using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.ViewModels;
using Moq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class DashboardSelectionSummaryTests
    {
        [Fact]
        public void SelectedRecordSummary_FollowsMostRecentActivitySelection()
        {
            using var db = new DatabaseService(":memory:");
            var vm = CreateViewModel(db);

            vm.SelectedCommonlyUsedItem = new ItemModel
            {
                ItemNumber = "DRILL-01",
                Name = "Hammer drill",
                Location = "Shelf A"
            };

            vm.SelectedActivity = new ActivityLog
            {
                Timestamp = new DateTime(2026, 6, 17, 9, 15, 0),
                UserName = "Alex",
                Action = "Imported items from CSV"
            };

            Assert.Contains("open Import / Export", vm.SelectedRecordSummary);
            Assert.Contains("Imported items from CSV", vm.SelectedRecordSummary);
            Assert.DoesNotContain("Hammer drill", vm.SelectedRecordSummary);
        }

        [Fact]
        public void SelectedRecordSummary_FollowsMostRecentRentalSelection()
        {
            using var db = new DatabaseService(":memory:");
            var vm = CreateViewModel(db);

            vm.SelectedActivity = new ActivityLog
            {
                Timestamp = new DateTime(2026, 6, 17, 9, 15, 0),
                UserName = "Alex",
                Action = "Reservation created for cordless saw"
            };
            vm.SelectedRental = new Rental
            {
                ItemNumber = "SAW-02",
                CustomerName = "Jordan Lee",
                DueDate = new DateTime(2026, 6, 24)
            };

            Assert.Contains("Rental: SAW-02 for Jordan Lee", vm.SelectedRecordSummary);
            Assert.DoesNotContain("Reservation created", vm.SelectedRecordSummary);
        }

        [Fact]
        public void OpenActivityDestinationCommand_IsDisabledUntilActivityIsSelected()
        {
            using var db = new DatabaseService(":memory:");
            var vm = CreateViewModel(db);

            Assert.False(vm.HasSelectedActivity);
            Assert.False(vm.OpenActivityDestinationCommand.CanExecute(null));

            vm.SelectedActivity = new ActivityLog
            {
                Timestamp = new DateTime(2026, 6, 17, 10, 0, 0),
                UserName = "Taylor",
                Action = "Rental returned"
            };

            Assert.True(vm.HasSelectedActivity);
            Assert.True(vm.OpenActivityDestinationCommand.CanExecute(null));
        }

        [Fact]
        public void SelectedActionCommands_ClearCanExecuteWhenSelectionIsCleared()
        {
            using var db = new DatabaseService(":memory:");
            var vm = CreateViewModel(db);

            vm.SelectedCheckedOutItem = new ItemModel
            {
                ItemID = 14,
                ItemNumber = "CORD-14",
                Name = "Extension cord",
                Location = "Bay 2",
                IsCheckedOut = true
            };

            Assert.True(vm.HasSelectedCheckedOutItem);
            Assert.True(vm.OpenSelectedCheckedOutItemCommand.CanExecute(null));
            Assert.True(vm.CheckInSelectedItemCommand.CanExecute(null));

            vm.SelectedCheckedOutItem = null;

            Assert.False(vm.HasSelectedCheckedOutItem);
            Assert.False(vm.OpenSelectedCheckedOutItemCommand.CanExecute(null));
            Assert.False(vm.CheckInSelectedItemCommand.CanExecute(null));
            Assert.Equal("Select or double-click a row to open the related workflow.", vm.SelectedRecordSummary);
        }

        [Fact]
        public async Task CheckInSelectedItemCommand_ClearsReturnedRowSelection()
        {
            using var db = new DatabaseService(":memory:");
            var itemService = new Mock<IItemService>();
            itemService.Setup(service => service.ToggleItemCheckOutStatusAsync(42, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var vm = CreateViewModel(db, itemService: itemService.Object);
            var item = new ItemModel
            {
                ItemID = 42,
                ItemNumber = "PUMP-42",
                Name = "Transfer pump",
                Location = "Rental shelf",
                IsCheckedOut = true
            };

            vm.CheckedOutItems.Add(item);
            vm.SelectedCheckedOutItem = item;

            await vm.CheckInSelectedItemCommand.ExecuteAsync(null);

            Assert.Null(vm.SelectedCheckedOutItem);
            Assert.DoesNotContain(item, vm.CheckedOutItems);
            Assert.False(vm.CheckInSelectedItemCommand.CanExecute(null));
            Assert.Equal("Select or double-click a row to open the related workflow.", vm.SelectedRecordSummary);
        }

        private static DashboardViewModel CreateViewModel(DatabaseService db, IItemService? itemService = null)
        {
            return new DashboardViewModel(
                itemService ?? Mock.Of<IItemService>(),
                Mock.Of<IRentalService>(),
                Mock.Of<ICustomerService>(),
                Mock.Of<IUserService>(),
                new ActivityLogService(db),
                new RelayCommand(() => { }),
                new RelayCommand(() => { }),
                new RelayCommand(() => { }));
        }
    }
}
