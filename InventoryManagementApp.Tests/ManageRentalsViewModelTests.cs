using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ManageRentalsViewModelTests
    {
        [Fact]
        public async Task SettingFilters_UpdatesResults()
        {
            var rentals = new List<RentalModel>
            {
                new RentalModel { RentalID = 1, ItemNumber = "A1", CustomerName = "John", RentalDate = DateTime.Today, Status = "Rented" },
                new RentalModel { RentalID = 2, ItemNumber = "B1", CustomerName = "Jane", RentalDate = DateTime.Today, Status = "Returned" }
            };
            var rentalService = new StubRentalService(rentals);
            var dialogService = new StubDialogService();
            var vm = new ManageRentalsViewModel(rentalService, dialogService);

            await vm.LoadRentalsAsync();
            Assert.Equal(2, vm.Rentals.Count);

            vm.SearchText = "A1";

            Assert.Single(vm.Rentals);
            Assert.Equal(1, vm.Rentals[0].RentalID);
        }

        private sealed class StubRentalService : IRentalService
        {
            private readonly List<RentalModel> _rentals;
            public StubRentalService(List<RentalModel> rentals) => _rentals = rentals;
            public Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate) => Task.CompletedTask;
            public Task ReturnItemAsync(int rentalID, DateTime returnDate) => Task.CompletedTask;
            public Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => Task.CompletedTask;
            public Task DeleteRentalAsync(int rentalID) => Task.CompletedTask;
            public Task<List<RentalModel>> GetActiveRentalsAsync() => Task.FromResult(new List<RentalModel>());
            public Task<int> CountActiveRentalsAsync() => Task.FromResult(0);
            public Task<List<RentalModel>> GetOverdueRentalsAsync() => Task.FromResult(new List<RentalModel>());
            public Task<List<RentalModel>> GetAllRentalsAsync() => Task.FromResult(_rentals);
            public Task<List<RentalModel>> GetRentalHistoryForItemAsync(int itemID) => Task.FromResult(new List<RentalModel>());
            public Task<List<RentalModel>> GetRentalHistoryForCustomerAsync(int customerID) => Task.FromResult(new List<RentalModel>());
            public Task<List<ItemRentalFrequency>> GetRentalFrequencyAsync(int topN = 10) => Task.FromResult(new List<ItemRentalFrequency>());
        }

        private sealed class StubDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public Task ShowInfoAsync(string message, string title) => Task.CompletedTask;
            public bool ShowConfirmation(string message, string title) => true;
            public Task<bool> ShowConfirmationAsync(string message, string title) => Task.FromResult(true);
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public Task<ItemModel?> ShowEditItemDialogAsync(ItemModel item) => Task.FromResult<ItemModel?>(null);
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }
    }
}

