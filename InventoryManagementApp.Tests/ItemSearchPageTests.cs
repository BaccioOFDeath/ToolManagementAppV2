using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Pages;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemSearchPageTests
    {
        [Fact]
        public void SearchBar_FiltersItems()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var host = Host.CreateDefaultBuilder()
                        .ConfigureServices(services =>
                        {
                            services.AddSingleton<IItemService, StubItemService>();
                            services.AddSingleton<IDialogService, DummyDialogService>();
                            services.AddSingleton<ICustomerService, DummyCustomerService>();
                            services.AddSingleton<IRentalService, DummyRentalService>();
                            services.AddSingleton<ISettingsService, DummySettingsService>();
                            services.AddSingleton<ItemManagementViewModel>();
                            services.AddSingleton<ILogger<ItemManagementViewModel>>(sp => NullLogger<ItemManagementViewModel>.Instance);
                        })
                        .Build();

                    var app = new App(host);
                    var vm = host.Services.GetRequiredService<ItemManagementViewModel>();
                    vm.LoadItemsAsync(new ItemPage(1, 50)).GetAwaiter().GetResult();

                    var page = new ItemSearchPage { DataContext = vm };
                    var searchBar = FindVisualChild<InventoryManagementApp.Controls.SearchBar>(page) ?? throw new InvalidOperationException("SearchBar not found");
                    searchBar.Text = "Screw";
                    if (searchBar.SearchCommand is CommunityToolkit.Mvvm.Input.IAsyncRelayCommand arc)
                        arc.ExecuteAsync(null).GetAwaiter().GetResult();
                    else
                        searchBar.SearchCommand.Execute(null);

                    SpinWait.SpinUntil(() => vm.SearchResults.Count == 1, TimeSpan.FromSeconds(5));
                    Assert.Single(vm.SearchResults);
                    Assert.Equal("Screwdriver", vm.SearchResults[0].Name);

                    app.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void ComboBox_FiltersByCategory()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var host = Host.CreateDefaultBuilder()
                        .ConfigureServices(services =>
                        {
                            services.AddSingleton<IItemService, StubItemService>();
                            services.AddSingleton<IDialogService, DummyDialogService>();
                            services.AddSingleton<ICustomerService, DummyCustomerService>();
                            services.AddSingleton<IRentalService, DummyRentalService>();
                            services.AddSingleton<ISettingsService, DummySettingsService>();
                            services.AddSingleton<ItemManagementViewModel>();
                            services.AddSingleton<ILogger<ItemManagementViewModel>>(sp => NullLogger<ItemManagementViewModel>.Instance);
                        })
                        .Build();

                    var app = new App(host);
                    var vm = host.Services.GetRequiredService<ItemManagementViewModel>();
                    vm.LoadItemsAsync(new ItemPage(1, 50)).GetAwaiter().GetResult();

                    var page = new ItemSearchPage { DataContext = vm };
                    var combo = FindVisualChild<ComboBox>(page) ?? throw new InvalidOperationException("ComboBox not found");
                    combo.SelectedItem = "Paints";

                    SpinWait.SpinUntil(() => vm.SearchResults.Count == 1 && vm.SearchResults[0].Brand == "Paints", TimeSpan.FromSeconds(5));
                    Assert.Single(vm.SearchResults);
                    Assert.Equal("Paints", vm.SearchResults[0].Brand);

                    app.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void UpdateState_ChangesVisualStateWithWidth()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var host = Host.CreateDefaultBuilder()
                        .ConfigureServices(services =>
                        {
                            services.AddSingleton<IDialogService, DummyDialogService>();
                            services.AddSingleton<ILogger<App>>(sp => NullLogger<App>.Instance);
                        })
                        .Build();

                    var app = new App(host);
                    var page = new ItemSearchPage();
                    page.Width = 700;
                    page.Height = 500;
                    page.Measure(new Size(page.Width, page.Height));
                    page.Arrange(new Rect(0, 0, page.Width, page.Height));
                    page.UpdateLayout();
                    page.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

                    var filterBar = (StackPanel)page.FindName("FilterBar")!;
                    var updateState = typeof(ItemSearchPage).GetMethod("UpdateState", BindingFlags.Instance | BindingFlags.NonPublic)!;
                    updateState.Invoke(page, null);

                    var groups = VisualStateManager.GetVisualStateGroups(page);
                    var state = ((VisualStateGroup)groups[0]).CurrentState?.Name;
                    Assert.Equal("Narrow", state);
                    Assert.Equal(Orientation.Vertical, filterBar.Orientation);

                    page.Width = 900;
                    page.Measure(new Size(page.Width, page.Height));
                    page.Arrange(new Rect(0, 0, page.Width, page.Height));
                    page.UpdateLayout();
                    updateState.Invoke(page, null);

                    groups = VisualStateManager.GetVisualStateGroups(page);
                    state = ((VisualStateGroup)groups[0]).CurrentState?.Name;
                    Assert.Equal("Wide", state);
                    Assert.Equal(Orientation.Horizontal, filterBar.Orientation);

                    app.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;
                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }

        private sealed class StubItemService : IItemService
        {
            private readonly List<ItemModel> _items = new()
            {
                new ItemModel { ItemID = 1, Name = "Hammer", Brand = "Tools" },
                new ItemModel { ItemID = 2, Name = "Screwdriver", Brand = "Tools" },
                new ItemModel { ItemID = 3, Name = "Paint", Brand = "Paints" }
            };

            public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult<ItemModel?>(null);
            public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default)
                => Enumerate(_items, cancellationToken);
            public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default)
                => Enumerate(_items.Where(i => i.Name.Contains(searchText ?? string.Empty, StringComparison.OrdinalIgnoreCase)), cancellationToken);
            public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(_items.Count);
            public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct) => Task.CompletedTask;
            public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult(false);
            public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task<List<ItemModel>> GetCheckedOutItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
            public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, SqliteConnection? conn = null, SqliteTransaction? tx = null, CancellationToken cancellationToken = default) => Task.CompletedTask;

            private async IAsyncEnumerable<ItemModel> Enumerate(IEnumerable<ItemModel> items, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
            {
                foreach (var item in items)
                {
                    ct.ThrowIfCancellationRequested();
                    yield return item;
                    await Task.Yield();
                }
            }
        }

        private sealed class DummyCustomerService : ICustomerService
        {
            public Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<Customer> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => Task.FromResult(new Customer());
            public Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Customer>());
            public Task<int> CountCustomersAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => Task.FromResult(new List<Customer>());
            public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default) => Task.FromResult(new CustomerImportResult());
            public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class DummyRentalService : IRentalService
        {
            public Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate) => Task.CompletedTask;
            public Task ReturnItemAsync(int rentalID, DateTime returnDate) => Task.CompletedTask;
            public Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => Task.CompletedTask;
            public Task DeleteRentalAsync(int rentalID) => Task.CompletedTask;
            public Task<List<Rental>> GetActiveRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<int> CountActiveRentalsAsync() => Task.FromResult(0);
            public Task<List<Rental>> GetOverdueRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetAllRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID) => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID) => Task.FromResult(new List<Rental>());
        }

        private sealed class DummyDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => true;
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        private sealed class DummySettingsService : ISettingsService
        {
            public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, string>());
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
            public Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
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
        }
    }
}

