using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemManagementViewModelCommandTests
    {
        [Fact]
        public async Task RentItemCommand_RentsItem()
        {
            var rental = new RecordingRentalService();
            var dialog = new RecordingDialogService
            {
                RentItemDialogResult = (new CustomerModel { CustomerID = 2 }, DateTime.Today)
            };
            var customer = new RecordingCustomerService();
            var settings = new DummySettingsService();
            var itemService = new ToggleItemService();
            var vm = new ItemManagementViewModel(itemService, customer, rental, dialog, settings, NullLogger<ItemManagementViewModel>.Instance);
            var item = new ItemModel { ItemID = 5 };

            await vm.RentItemCommand.ExecuteAsync(item);

            Assert.True(customer.GetAllCalled);
            Assert.True(dialog.RentItemDialogCalled);
            Assert.True(rental.RentCalled);
            Assert.Equal(5, rental.ItemId);
            Assert.Equal(2, rental.CustomerId);
        }

        [Fact]
        public async Task ToggleCheckOutCommand_TogglesItem()
        {
            var itemService = new RecordingToggleItemService
            {
                GetItemResult = new ItemModel { ItemID = 1, QuantityOnHand = 3, IsCheckedOut = false }
            };
            var dialog = new RecordingDialogService();
            var rental = new RecordingRentalService();
            var customer = new RecordingCustomerService();
            var settings = new DummySettingsService();
            var vm = new ItemManagementViewModel(itemService, customer, rental, dialog, settings, NullLogger<ItemManagementViewModel>.Instance);
            var item = new ItemModel { ItemID = 1, QuantityOnHand = 3 };

            await vm.ToggleCheckOutCommand.ExecuteAsync(item);

            Assert.True(itemService.ToggleCalled);
            Assert.True(item.IsCheckedOut);
            Assert.Equal(2, item.QuantityOnHand);

            await vm.ToggleCheckOutCommand.ExecuteAsync(item);

            Assert.False(item.IsCheckedOut);
            Assert.Equal(3, item.QuantityOnHand);
        }

        [Fact]
        public async Task ToggleCheckOutCommand_RentedItemPromptsForRentalReturnInsteadOfToggling()
        {
            var itemService = new RecordingToggleItemService
            {
                GetItemResult = new ItemModel { ItemID = 80, ItemNumber = "T30", QuantityOnHand = 0, RentedQuantity = 1, IsCheckedOut = false }
            };
            var dialog = new RecordingDialogService();
            var rental = new RecordingRentalService();
            var customer = new RecordingCustomerService();
            var settings = new DummySettingsService();
            var vm = new ItemManagementViewModel(itemService, customer, rental, dialog, settings, NullLogger<ItemManagementViewModel>.Instance);
            var item = new ItemModel { ItemID = 80, ItemNumber = "T30", QuantityOnHand = 0, RentedQuantity = 1, IsCheckedOut = false };
            var openedRentals = false;
            vm.SelectedItem = item;
            vm.OpenRentalReturnWorkflowAsync = () =>
            {
                openedRentals = true;
                return Task.CompletedTask;
            };

            await vm.ToggleCheckOutCommand.ExecuteAsync(item);

            Assert.False(itemService.ToggleCalled);
            Assert.Equal(1, dialog.ConfirmCalls);
            Assert.Equal("Return Rental", dialog.LastConfirmTitle);
            Assert.Contains("currently rented out, not checked out", dialog.LastConfirmMessage);
            Assert.True(openedRentals);
        }

        [Fact]
        public void OpenItemCardCommand_SelectsItemAndShowsDetails()
        {
            var itemService = new ToggleItemService();
            var dialog = new RecordingDialogService();
            var rental = new RecordingRentalService();
            var customer = new RecordingCustomerService();
            var settings = new DummySettingsService();
            var vm = new ItemManagementViewModel(itemService, customer, rental, dialog, settings, NullLogger<ItemManagementViewModel>.Instance);
            var item = new ItemModel { ItemID = 9, Name = "Torque Wrench" };

            vm.OpenItemCardCommand.Execute(item);

            Assert.Same(item, vm.SelectedItem);
            Assert.True(dialog.ShowItemDetailsCalled);
            Assert.Same(item, dialog.LastDetailedItem);
        }

        [Fact]
        public async Task ItemDetailsViewModel_RentalHistoryCommand_ShowsHistory()
        {
            var rental = new RecordingRentalService();
            var dialog = new RecordingDialogService();
            var itemService = new RecordingToggleItemService();
            var customer = new RecordingCustomerService();
            var item = new ItemModel { ItemID = 12, Name = "Brake Bleeder" };
            var vm = new ItemDetailsViewModel(item, itemService, customer, rental, dialog, () => { });

            await vm.OpenRentalHistoryCommand.ExecuteAsync(null);

            Assert.Equal(12, rental.LastHistoryItemId);
            Assert.True(dialog.ShowRentalHistoryCalled);
            Assert.Same(item, dialog.LastHistoryItem);
        }

        [Fact]
        public async Task ItemDetailsViewModel_CheckoutHistoryCommand_ShowsCheckoutActivity()
        {
            var rental = new RecordingRentalService();
            var dialog = new RecordingDialogService();
            var itemService = new RecordingToggleItemService();
            var customer = new RecordingCustomerService();
            var activityLog = new RecordingActivityLogService(new List<ActivityLog>
            {
                new()
                {
                    UserName = "Alex",
                    Action = "Checked out item T34 (12)",
                    Timestamp = new DateTime(2026, 6, 26, 9, 30, 0)
                },
                new()
                {
                    UserName = "Sam",
                    Action = "Checked in item T34 (12)",
                    Timestamp = new DateTime(2026, 6, 26, 12, 45, 0)
                }
            });
            var item = new ItemModel { ItemID = 12, ItemNumber = "T34", Name = "Brake Bleeder" };
            var vm = new ItemDetailsViewModel(item, itemService, customer, rental, dialog, () => { }, activityLogService: activityLog);

            await vm.OpenCheckoutHistoryCommand.ExecuteAsync(null);

            Assert.Equal(12, activityLog.LastItemId);
            Assert.Equal("T34", activityLog.LastItemNumber);
            Assert.Equal("Checkout History - T34", dialog.LastInfoTitle);
            Assert.Contains("2026-06-26 12:45 - Sam - Checked in item T34 (12)", dialog.LastInfoMessage);
            Assert.Contains("2026-06-26 09:30 - Alex - Checked out item T34 (12)", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task ItemDetailsViewModel_EditCommand_UpdatesItem()
        {
            var rental = new RecordingRentalService();
            var dialog = new RecordingDialogService();
            var itemService = new RecordingToggleItemService();
            var customer = new RecordingCustomerService();
            var item = new ItemModel { ItemID = 3, Name = "Original", Location = "A1" };
            dialog.EditItemDialogResult = new ItemModel { ItemID = 3, Name = "Updated", Location = "B9" };
            var vm = new ItemDetailsViewModel(item, itemService, customer, rental, dialog, () => { });

            await vm.EditCommand.ExecuteAsync(null);

            Assert.True(itemService.UpdateCalled);
            Assert.Equal("Updated", item.Name);
            Assert.Equal("B9", item.Location);
        }

        [Fact]
        public async Task ItemDetailsViewModel_ToggleCheckOutCommand_UpdatesItemState()
        {
            var rental = new RecordingRentalService();
            var dialog = new RecordingDialogService();
            var itemService = new RecordingToggleItemService
            {
                GetItemResult = new ItemModel
                {
                    ItemID = 4,
                    QuantityOnHand = 2,
                    IsCheckedOut = false,
                    CheckedOutBy = "Alex",
                    CheckedOutTime = new DateTime(2026, 4, 28, 10, 0, 0)
                }
            };
            var customer = new RecordingCustomerService();
            var item = new ItemModel { ItemID = 4, QuantityOnHand = 2, IsCheckedOut = false };
            var vm = new ItemDetailsViewModel(item, itemService, customer, rental, dialog, () => { });

            await vm.ToggleCheckOutCommand.ExecuteAsync(null);

            Assert.True(itemService.ToggleCalled);
            Assert.True(item.IsCheckedOut);
            Assert.Equal(1, item.QuantityOnHand);
            Assert.Equal("Alex", item.CheckedOutBy);
            Assert.Equal("Check In", vm.CheckOutButtonText);
        }

        [Fact]
        public async Task ItemDetailsViewModel_ToggleCheckOutCommand_RentedItemExplainsRentalReturnPath()
        {
            var rental = new RecordingRentalService();
            var dialog = new RecordingDialogService();
            var itemService = new RecordingToggleItemService
            {
                GetItemResult = new ItemModel { ItemID = 80, ItemNumber = "T30", QuantityOnHand = 0, RentedQuantity = 1, IsCheckedOut = false }
            };
            var customer = new RecordingCustomerService();
            var item = new ItemModel { ItemID = 80, ItemNumber = "T30", QuantityOnHand = 0, RentedQuantity = 1, IsCheckedOut = false };
            var vm = new ItemDetailsViewModel(item, itemService, customer, rental, dialog, () => { });

            await vm.ToggleCheckOutCommand.ExecuteAsync(null);

            Assert.False(itemService.ToggleCalled);
            Assert.Equal("Return Rental", dialog.LastInfoTitle);
            Assert.Contains("currently rented out, not checked out", dialog.LastInfoMessage);
            Assert.Contains("Open Rentals to return", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task ItemDetailsViewModel_RentOutCommand_RentsItemAndRefreshesState()
        {
            var rental = new RecordingRentalService();
            var dialog = new RecordingDialogService
            {
                RentItemDialogResult = (new CustomerModel { CustomerID = 8 }, DateTime.Today.AddDays(3))
            };
            var itemService = new RecordingToggleItemService
            {
                GetItemResult = new ItemModel
                {
                    ItemID = 6,
                    QuantityOnHand = 1,
                    RentedQuantity = 1,
                    Name = "Updated after rent"
                }
            };
            var customer = new RecordingCustomerService();
            var item = new ItemModel { ItemID = 6, QuantityOnHand = 2, RentedQuantity = 0, Name = "Before rent" };
            var vm = new ItemDetailsViewModel(item, itemService, customer, rental, dialog, () => { });

            await vm.RentOutCommand.ExecuteAsync(null);

            Assert.True(customer.GetAllCalled);
            Assert.True(dialog.RentItemDialogCalled);
            Assert.True(rental.RentCalled);
            Assert.Equal(6, rental.ItemId);
            Assert.Equal(8, rental.CustomerId);
            Assert.Equal(1, item.QuantityOnHand);
            Assert.Equal(1, item.RentedQuantity);
            Assert.Equal("Updated after rent", item.Name);
        }

        [Fact]
        public async Task ItemDetailsViewModel_RentOutCommand_PromptsToPrintRentalHandoff()
        {
            var dueDate = DateTime.Today.AddDays(3);
            var rental = new RecordingRentalService();
            rental.ActiveRentals.Add(new RentalModel
            {
                RentalID = 42,
                ItemID = 6,
                CustomerID = 8,
                ItemNumber = "T6",
                ItemLocation = "A1",
                CustomerName = "SD European",
                RentalDate = DateTime.Today,
                DueDate = dueDate,
                Status = "Rented"
            });
            var dialog = new RecordingDialogService
            {
                RentItemDialogResult = (new CustomerModel { CustomerID = 8, Company = "SD European" }, dueDate)
            };
            var itemService = new RecordingToggleItemService
            {
                GetItemResult = new ItemModel { ItemID = 6, QuantityOnHand = 1, RentedQuantity = 1, Name = "Updated after rent" }
            };
            var customer = new RecordingCustomerService();
            var item = new ItemModel { ItemID = 6, ItemNumber = "T6", Location = "A1", QuantityOnHand = 2, RentedQuantity = 0 };
            var settings = new DummySettingsService();
            await settings.SaveSettingAsync("Rental.InvoiceEnabled", "True");
            var vm = new ItemDetailsViewModel(item, itemService, customer, rental, dialog, () => { }, null, settings);

            await vm.RentOutCommand.ExecuteAsync(null);

            Assert.Equal(1, dialog.ConfirmCalls);
            Assert.Equal("Print Rental Handoff", dialog.LastConfirmTitle);
            Assert.Contains("SD European", dialog.LastConfirmMessage);
            Assert.Equal(2, dialog.PrintPreviewCalls);
            Assert.Contains("Picking Slip - Rental 42", dialog.PrintPreviewTitles);
            Assert.Contains("Invoice - Rental 42", dialog.PrintPreviewTitles);
        }

        [Fact]
        public async Task ItemDetailsViewModel_RentOutCommand_SkipsInvoiceWhenInvoiceSettingIsOff()
        {
            var dueDate = DateTime.Today.AddDays(3);
            var rental = new RecordingRentalService();
            rental.ActiveRentals.Add(new RentalModel
            {
                RentalID = 42,
                ItemID = 6,
                CustomerID = 8,
                ItemNumber = "T6",
                ItemLocation = "A1",
                CustomerName = "SD European",
                RentalDate = DateTime.Today,
                DueDate = dueDate,
                Status = "Rented"
            });
            var dialog = new RecordingDialogService
            {
                RentItemDialogResult = (new CustomerModel { CustomerID = 8, Company = "SD European" }, dueDate)
            };
            var itemService = new RecordingToggleItemService
            {
                GetItemResult = new ItemModel { ItemID = 6, QuantityOnHand = 1, RentedQuantity = 1, Name = "Updated after rent" }
            };
            var customer = new RecordingCustomerService();
            var item = new ItemModel { ItemID = 6, ItemNumber = "T6", Location = "A1", QuantityOnHand = 2, RentedQuantity = 0 };
            var vm = new ItemDetailsViewModel(item, itemService, customer, rental, dialog, () => { }, null, new DummySettingsService());

            await vm.RentOutCommand.ExecuteAsync(null);

            Assert.Equal(1, dialog.ConfirmCalls);
            Assert.Contains("Print the picking slip for shelf collection now?", dialog.LastConfirmMessage);
            Assert.Equal(1, dialog.PrintPreviewCalls);
            Assert.Contains("Picking Slip - Rental 42", dialog.PrintPreviewTitles);
            Assert.DoesNotContain("Invoice - Rental 42", dialog.PrintPreviewTitles);
        }

        [Fact]
        public async Task ItemDetailsViewModel_RentOutCommand_DoesNotPrintWhenHandoffPromptIsCancelled()
        {
            var dueDate = DateTime.Today.AddDays(3);
            var rental = new RecordingRentalService();
            var dialog = new RecordingDialogService
            {
                ConfirmResult = false,
                RentItemDialogResult = (new CustomerModel { CustomerID = 8, Company = "SD European" }, dueDate)
            };
            var itemService = new RecordingToggleItemService
            {
                GetItemResult = new ItemModel { ItemID = 6, QuantityOnHand = 1, RentedQuantity = 1, Name = "Updated after rent" }
            };
            var customer = new RecordingCustomerService();
            var item = new ItemModel { ItemID = 6, ItemNumber = "T6", Location = "A1", QuantityOnHand = 2, RentedQuantity = 0 };
            var vm = new ItemDetailsViewModel(item, itemService, customer, rental, dialog, () => { });

            await vm.RentOutCommand.ExecuteAsync(null);

            Assert.Equal(1, dialog.ConfirmCalls);
            Assert.Equal(0, dialog.PrintPreviewCalls);
        }

        [Fact]
        public async Task ItemDetailsViewModel_ToggleCheckOutCommand_HandlesMissingPersistedItem()
        {
            var rental = new RecordingRentalService();
            var dialog = new RecordingDialogService();
            var itemService = new RecordingToggleItemService
            {
                ToggleException = new InvalidOperationException("Item 132 not found.")
            };
            var customer = new RecordingCustomerService();
            var item = new ItemModel { ItemID = 132, QuantityOnHand = 1, IsCheckedOut = false };
            var vm = new ItemDetailsViewModel(item, itemService, customer, rental, dialog, () => { });

            await vm.ToggleCheckOutCommand.ExecuteAsync(null);

            Assert.True(itemService.ToggleCalled);
            Assert.Contains("Failed to update check-out status: Item 132 not found.", dialog.LastInfoMessage);
            Assert.Equal("Error", dialog.LastInfoTitle);
            Assert.False(item.IsCheckedOut);
        }

        [Fact]
        public async Task ItemDetailsViewModel_RentOutCommand_HandlesInsufficientQuantity()
        {
            var rental = new RecordingRentalService
            {
                RentException = new InvalidOperationException("Insufficient quantity.")
            };
            var dialog = new RecordingDialogService
            {
                RentItemDialogResult = (new CustomerModel { CustomerID = 8 }, DateTime.Today.AddDays(3))
            };
            var itemService = new RecordingToggleItemService
            {
                GetItemResult = new ItemModel { ItemID = 6, QuantityOnHand = 0, RentedQuantity = 1, Name = "Persisted" }
            };
            var customer = new RecordingCustomerService();
            var item = new ItemModel { ItemID = 6, QuantityOnHand = 0, RentedQuantity = 1, Name = "Before rent" };
            var vm = new ItemDetailsViewModel(item, itemService, customer, rental, dialog, () => { });

            await vm.RentOutCommand.ExecuteAsync(null);

            Assert.True(rental.RentCalled);
            Assert.Contains("Failed to rent item: Insufficient quantity.", dialog.LastInfoMessage);
            Assert.Equal("Error", dialog.LastInfoTitle);
            Assert.Equal("Persisted", item.Name);
        }

        private sealed class RecordingRentalService : IRentalService
        {
            public bool RentCalled { get; private set; }
            public int ItemId { get; private set; }
            public int CustomerId { get; private set; }
            public int LastHistoryItemId { get; private set; }
            public Exception? RentException { get; set; }
            public List<RentalModel> ActiveRentals { get; } = new();
            public Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate)
            {
                RentCalled = true;
                ItemId = itemID;
                CustomerId = customerID;
                if (RentException != null) throw RentException;
                return Task.CompletedTask;
            }
            public Task ReturnItemAsync(int rentalID, DateTime returnDate) => Task.CompletedTask;
            public Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => Task.CompletedTask;
            public Task DeleteRentalAsync(int rentalID) => Task.CompletedTask;
            public Task<List<Rental>> GetActiveRentalsAsync() => Task.FromResult(ActiveRentals.Cast<Rental>().ToList());
            public Task<int> CountActiveRentalsAsync() => Task.FromResult(0);
            public Task<List<Rental>> GetOverdueRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetAllRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID)
            {
                LastHistoryItemId = itemID;
                return Task.FromResult(new List<Rental>());
            }
            public Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID) => Task.FromResult(new List<Rental>());
            public Task<List<ItemRentalFrequency>> GetRentalFrequencyAsync(int topN = 10) => Task.FromResult(new List<ItemRentalFrequency>());
        }

        private sealed class RecordingCustomerService : ICustomerService
        {
            public bool GetAllCalled { get; private set; }
            public Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<Customer?> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => Task.FromResult<Customer?>(new Customer());
            public Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
            {
                GetAllCalled = true;
                return Task.FromResult(new List<Customer> { new Customer { CustomerID = 2 } });
            }
            public Task<int> CountCustomersAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => Task.FromResult(new List<Customer>());
            public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default) => Task.FromResult(new CustomerImportResult());
            public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> ImportCustomersAsync(string filePath, IDataImporter<Customer> importer, CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task ExportCustomersAsync(string filePath, IDataExporter<Customer> exporter, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class RecordingActivityLogService : ActivityLogService
        {
            readonly List<ActivityLog> _logs;
            public int LastItemId { get; private set; }
            public string? LastItemNumber { get; private set; }

            public RecordingActivityLogService(List<ActivityLog> logs)
                : base(new DatabaseService(":memory:", NullLogger<DatabaseService>.Instance))
            {
                _logs = logs;
            }

            public override Task<Result<List<ActivityLog>>> GetCheckoutHistoryForItemAsync(int itemID, string? itemNumber, CancellationToken cancellationToken = default)
            {
                LastItemId = itemID;
                LastItemNumber = itemNumber;
                return Task.FromResult(new Result<List<ActivityLog>>(_logs, true));
            }
        }

        private sealed class RecordingDialogService : IDialogService
        {
            public bool RentItemDialogCalled { get; private set; }
            public bool ShowItemDetailsCalled { get; private set; }
            public bool ShowRentalHistoryCalled { get; private set; }
            public ItemModel? LastDetailedItem { get; private set; }
            public ItemModel? LastHistoryItem { get; private set; }
            public (CustomerModel customer, DateTime dueDate)? RentItemDialogResult { get; set; }
            public ItemModel? EditItemDialogResult { get; set; }
            public bool ConfirmResult { get; set; } = true;
            public int ConfirmCalls { get; private set; }
            public string LastConfirmTitle { get; private set; } = string.Empty;
            public string LastConfirmMessage { get; private set; } = string.Empty;
            public int PrintPreviewCalls { get; private set; }
            public List<string> PrintPreviewTitles { get; } = new();
            public string LastInfoMessage { get; private set; } = string.Empty;
            public string LastInfoTitle { get; private set; } = string.Empty;
            public void ShowInfo(string message, string title)
            {
                LastInfoMessage = message;
                LastInfoTitle = title;
            }
            public Task ShowInfoAsync(string message, string title)
            {
                ShowInfo(message, title);
                return Task.CompletedTask;
            }
            public bool ShowConfirmation(string message, string title)
            {
                ConfirmCalls++;
                LastConfirmTitle = title;
                LastConfirmMessage = message;
                return ConfirmResult;
            }
            public ItemModel? ShowEditItemDialog(ItemModel item) => EditItemDialogResult;
            public Task<ItemModel?> ShowEditItemDialogAsync(ItemModel item) => Task.FromResult(EditItemDialogResult);
            public void ShowItemDetails(ItemModel item)
            {
                ShowItemDetailsCalled = true;
                LastDetailedItem = item;
            }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers)
            {
                RentItemDialogCalled = true;
                return RentItemDialogResult;
            }
            public CustomerModel? ShowAddCustomerDialog() => null;
            public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history)
            {
                ShowRentalHistoryCalled = true;
                LastHistoryItem = item;
            }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description)
            {
                PrintPreviewCalls++;
                PrintPreviewTitles.Add(title);
            }
            public void ShowPrintLabelDialog() { }
        }

        private sealed class DummySettingsService : ISettingsService
        {
            readonly Dictionary<string, string> _settings = new();
            public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
            public event EventHandler<double>? ItemCardSizeChanged;
            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
            {
                _settings[key] = value;
                return Task.CompletedTask;
            }
            public Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default)
                => Task.FromResult(key != null && _settings.TryGetValue(key, out var value) ? value : null);
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, string>(_settings));
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
            {
                foreach (var setting in settings)
                    _settings[setting.Key] = setting.Value;
                return Task.CompletedTask;
            }
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default)
            {
                _settings.Remove(key);
                return Task.CompletedTask;
            }
            public Task<string?> GetThemeAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
            public Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default) => Task.FromResult<IDictionary<ItemDetailField, bool>>(Enum.GetValues<ItemDetailField>().ToDictionary(f => f, _ => true));
            public Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default)
            {
                ItemDetailVisibilityChanged?.Invoke(this, visibility);
                return Task.CompletedTask;
            }
            public Task<double> GetItemCardSizeAsync(CancellationToken cancellationToken = default) => Task.FromResult(1.0);
            public Task SaveItemCardSizeAsync(double size, CancellationToken cancellationToken = default)
            {
                ItemCardSizeChanged?.Invoke(this, size);
                return Task.CompletedTask;
            }
        }

        private sealed class ToggleItemService : IItemService
        {
            public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult<ItemModel?>(null);
            public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
            public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct) => Task.CompletedTask;
            public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult(true);
            public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task<List<ItemModel>> GetCheckedOutItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
            public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<int>> ImportItemsAsync(string filePath, IDataImporter<ItemModel> importer, CancellationToken cancellationToken = default) => Task.FromResult(new List<int>());
            public Task ExportItemsAsync(string filePath, IDataExporter<ItemModel> exporter, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, Microsoft.Data.Sqlite.SqliteConnection? conn = null, Microsoft.Data.Sqlite.SqliteTransaction? tx = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<ItemModel>> GetMostCommonlyUsedItemsAsync(int limit, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task<List<ItemModel>> GetIncompleteItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        }

        private sealed class RecordingToggleItemService : IItemService
        {
            public bool ToggleCalled { get; private set; }
            public bool UpdateCalled { get; private set; }
            public ItemModel? UpdatedItem { get; private set; }
            public ItemModel? GetItemResult { get; set; }
            public Exception? ToggleException { get; set; }
            public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default)
            {
                UpdateCalled = true;
                UpdatedItem = item;
                return Task.CompletedTask;
            }
            public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult(GetItemResult);
            public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
            public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct) => Task.CompletedTask;
            public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, CancellationToken cancellationToken = default)
            {
                ToggleCalled = true;
                if (ToggleException != null) throw ToggleException;
                if (GetItemResult != null)
                {
                    var checkedOut = !GetItemResult.IsCheckedOut;
                    GetItemResult.IsCheckedOut = checkedOut;
                    GetItemResult.QuantityOnHand += checkedOut ? -1 : 1;
                    if (checkedOut && string.IsNullOrWhiteSpace(GetItemResult.CheckedOutBy))
                        GetItemResult.CheckedOutBy = "Alex";
                }
                return Task.FromResult(true);
            }
            public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task<List<ItemModel>> GetCheckedOutItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
            public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<int>> ImportItemsAsync(string filePath, IDataImporter<ItemModel> importer, CancellationToken cancellationToken = default) => Task.FromResult(new List<int>());
            public Task ExportItemsAsync(string filePath, IDataExporter<ItemModel> exporter, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, Microsoft.Data.Sqlite.SqliteConnection? conn = null, Microsoft.Data.Sqlite.SqliteTransaction? tx = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<ItemModel>> GetMostCommonlyUsedItemsAsync(int limit, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task<List<ItemModel>> GetIncompleteItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        }
    }
}

