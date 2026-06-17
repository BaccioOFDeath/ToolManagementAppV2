using System;
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

        private static DashboardViewModel CreateViewModel(DatabaseService db)
        {
            return new DashboardViewModel(
                Mock.Of<IItemService>(),
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
