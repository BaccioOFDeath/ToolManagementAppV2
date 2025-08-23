using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.ViewModels;
using Xunit;

public class ItemDisplaySettingsTests
{
    private sealed class DummyItemService : IItemService
    {
        public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult<ItemModel?>(null);
        public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
        public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
        public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
        public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct) => Task.CompletedTask;
        public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, string currentUser, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task<List<ItemModel>> GetCheckedOutItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
        public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
        public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
        public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, Microsoft.Data.Sqlite.SqliteConnection? conn = null, Microsoft.Data.Sqlite.SqliteTransaction? tx = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
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
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
        public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }

    [Fact]
    public async Task ItemManagementViewModel_ReflectsDisplaySettings()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        var settings = new SettingsService(db);
        await settings.SaveShowItemImageAsync(false);
        await settings.SaveShowItemNameAsync(false);
        await settings.SaveShowItemNumberAsync(false);
        await settings.SaveShowItemLocationAsync(false);
        await settings.SaveShowItemNotesAsync(false);
        var vmFalse = new ItemManagementViewModel(new DummyItemService(), new DummyCustomerService(), new DummyRentalService(), new DummyDialogService(), settings);
        Assert.False(vmFalse.ShowImage);
        Assert.False(vmFalse.ShowName);
        Assert.False(vmFalse.ShowItemNumber);
        Assert.False(vmFalse.ShowLocation);
        Assert.False(vmFalse.ShowNotes);
        await settings.SaveShowItemImageAsync(true);
        await settings.SaveShowItemNameAsync(true);
        await settings.SaveShowItemNumberAsync(true);
        await settings.SaveShowItemLocationAsync(true);
        await settings.SaveShowItemNotesAsync(true);
        var vmTrue = new ItemManagementViewModel(new DummyItemService(), new DummyCustomerService(), new DummyRentalService(), new DummyDialogService(), settings);
        Assert.True(vmTrue.ShowImage);
        Assert.True(vmTrue.ShowName);
        Assert.True(vmTrue.ShowItemNumber);
        Assert.True(vmTrue.ShowLocation);
        Assert.True(vmTrue.ShowNotes);
    }

    [Fact]
    public void ItemCardTemplate_VisibilityFollowsSettings()
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
                using var db = new DatabaseService(dbPath);
                var settings = new SettingsService(db);
                settings.SaveShowItemImageAsync(false).GetAwaiter().GetResult();
                settings.SaveShowItemNameAsync(false).GetAwaiter().GetResult();
                settings.SaveShowItemNumberAsync(false).GetAwaiter().GetResult();
                settings.SaveShowItemLocationAsync(false).GetAwaiter().GetResult();
                settings.SaveShowItemNotesAsync(false).GetAwaiter().GetResult();
                var vm = new ItemManagementViewModel(new DummyItemService(), new DummyCustomerService(), new DummyRentalService(), new DummyDialogService(), settings);
                var app = new Application();
                app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Converters.xaml", UriKind.Absolute) });
                app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Styles.xaml", UriKind.Absolute) });
                app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Templates.xaml", UriKind.Absolute) });
                var template = (DataTemplate)app.Resources["ItemCardTemplate"];
                var item = new ItemModel { Name = "A", ItemNumber = "1", Location = "L", Notes = "N" };
                var itemsControl = new ItemsControl { DataContext = vm, ItemTemplate = template };
                itemsControl.Items.Add(item);
                var window = new Window { Content = itemsControl };
                window.Show();
                itemsControl.UpdateLayout();
                var container = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromIndex(0)!;
                container.ApplyTemplate();
                var border = (Border)VisualTreeHelper.GetChild(container, 0);
                Assert.Equal(Visibility.Collapsed, border.Visibility);
                settings.SaveShowItemNameAsync(true).GetAwaiter().GetResult();
                vm = new ItemManagementViewModel(new DummyItemService(), new DummyCustomerService(), new DummyRentalService(), new DummyDialogService(), settings);
                itemsControl.DataContext = vm;
                itemsControl.UpdateLayout();
                border = (Border)VisualTreeHelper.GetChild(container, 0);
                Assert.Equal(Visibility.Visible, border.Visibility);
                window.Close();
            }
            catch (Exception ex)
            {
                threadEx = ex;
            }
            finally
            {
                Application.Current?.Shutdown();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (threadEx != null) throw threadEx;
    }
}
