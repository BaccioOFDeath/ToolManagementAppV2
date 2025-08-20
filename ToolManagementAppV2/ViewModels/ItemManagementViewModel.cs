using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities;
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Utilities.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.ViewModels
{
    public class ItemManagementViewModel : ObservableObject, IDisposable
    {
        private readonly IItemService _itemService;
        private readonly ICustomerService _customerService;
        private readonly IRentalService _rentalService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ItemManagementViewModel> _logger;

        public ObservableCollection<ItemModel> Tools { get; } = new();
        public ObservableCollection<ItemModel> SearchResults { get; } = new();

        /// <summary>
        /// List of available tool categories derived from distinct brands
        /// in the current tool set; rebuilt whenever tools are loaded or filtered.
        /// </summary>
        public ObservableCollection<string> Categories { get; } = new();

        private ItemModel _newTool = new();
        public ItemModel NewTool
        {
            get => _newTool;
            set => SetProperty(ref _newTool, value);
        }

        private string _searchTerm = string.Empty;
        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        private ItemModel _selectedItem;
        public ItemModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    ((AsyncRelayCommand)OpenRentalsCommand).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)EditToolCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)ViewDetailsCommand).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)OpenRentalHistoryCommand).NotifyCanExecuteChanged();
                }
            }
        }

        private string _selectedCategory = "All";

        /// <summary>
        /// Currently selected category used to filter <see cref="SearchResults"/>.
        /// Changing the value triggers <see cref="SearchCommand"/> to reapply the filter.
        /// </summary>
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    _searchCts.Cancel();
                    _searchCts.Dispose();
                    _searchCts = new CancellationTokenSource();
                    SearchCommand.Execute(_searchCts.Token);
                }
            }
        }

        public IAsyncRelayCommand<CancellationToken> SearchCommand { get; }
        public IAsyncRelayCommand NewToolCommand { get; }
        public IAsyncRelayCommand EditToolCommand { get; }
        public IAsyncRelayCommand DeleteToolCommand { get; }
        public IAsyncRelayCommand OpenRentalsCommand { get; }
        public IRelayCommand ViewDetailsCommand { get; }
        public IAsyncRelayCommand OpenRentalHistoryCommand { get; }

        readonly IDispatcherTimer _searchDebounceTimer;
        CancellationTokenSource _searchCts = new();

        bool _suppressToolsChanged;
        bool _disposed;

        // Writable for TwoWay binding from XAML. Mirrors SearchTerm and triggers search.
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    SearchTerm = value;
                    _searchCts.Cancel();
                    _searchDebounceTimer.Stop();
                    _searchDebounceTimer.Start();
                }
            }
        }

        public ItemManagementViewModel(IItemService itemService,
                                       ICustomerService customerService,
                                       IRentalService rentalService,
                                       IDialogService dialogService,
                                       ILogger<ItemManagementViewModel>? logger = null,
                                       IDispatcherTimer? searchDebounceTimer = null)
        {
            _itemService = itemService;
            _customerService = customerService;
            _rentalService = rentalService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<ItemManagementViewModel>.Instance;
            SearchCommand = new AsyncRelayCommand<CancellationToken>(FilterToolsAsync);
            _searchDebounceTimer = searchDebounceTimer ?? new DispatcherTimerWrapper { Interval = TimeSpan.FromMilliseconds(300) };
            _searchDebounceTimer.Tick += OnSearchDebounceTimerTick;
            NewToolCommand = new AsyncRelayCommand(ct => AddToolAsync(ct));
            EditToolCommand = new AsyncRelayCommand(ct => EditToolAsync(ct), () => SelectedItem != null);
            DeleteToolCommand = new AsyncRelayCommand(ct => DeleteToolAsync(ct));
            OpenRentalsCommand = new AsyncRelayCommand(ct => OpenRentalsAsync(ct), () => SelectedItem != null);
            ViewDetailsCommand = new RelayCommand(ViewDetails, () => SelectedItem != null);
            OpenRentalHistoryCommand = new AsyncRelayCommand(OpenRentalHistoryAsync, () => SelectedItem != null);
            // Ensure no duplicate event subscriptions when the view model is
            // constructed multiple times or the collection persists across
            // instances.
            Tools.CollectionChanged -= Tools_CollectionChanged;
            Tools.CollectionChanged += Tools_CollectionChanged;
        }

        void OnSearchDebounceTimerTick(object? s, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchCts.Dispose();
            _searchCts = new CancellationTokenSource();
            SearchCommand.Execute(_searchCts.Token);
        }

        public async Task LoadToolsAsync()
        {
            _suppressToolsChanged = true;
            try
            {
                var all = await _itemService.GetAllToolsAsync();
                Tools.ReplaceRange(all);
                SearchResults.ReplaceRange(all);
                LoadCategories(Tools);
            }
            finally
            {
                _suppressToolsChanged = false;
            }
        }

        /// <summary>
        /// Applies text and category filters to the tool list.
        /// Invoked by <see cref="SearchCommand"/> whenever the search text or
        /// <see cref="SelectedCategory"/> changes and recomputes <see cref="Categories"/>.
        /// </summary>
        async Task FilterToolsAsync(CancellationToken cancellationToken)
        {
            var term = string.IsNullOrWhiteSpace(SearchTerm) ? string.Empty : SearchTerm.Trim();
            IEnumerable<ItemModel> source;

            if (!string.IsNullOrEmpty(term))
            {
                source = await _itemService.SearchToolsAsync(term, cancellationToken);
            }
            else
            {
                if (Tools.Count == 0)
                {
                    var all = await _itemService.GetAllToolsAsync();
                    Tools.ReplaceRange(all);
                }
                source = Tools;
            }

            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                source = source.Where(t => string.Equals(t.Brand, SelectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            SearchResults.ReplaceRange(source);
            LoadCategories(source, suppressSearch: true);
        }

        async Task AddToolAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewTool.ToolNumber))
                    NewTool.ToolNumber = await _itemService.GenerateNextToolNumberAsync(cancellationToken);
                await _itemService.AddToolAsync(NewTool, cancellationToken);
                await LoadToolsAsync();
                await FilterToolsAsync(cancellationToken);
                NewTool = new ItemModel();
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync($"You are not authorized to add {LabelProvider.Instance.ItemLabelPlural.ToLower()}.", "Unauthorized");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to add {ItemLabelSingular} due to invalid operation", LabelProvider.Instance.ItemLabelSingular);
                await _dialogService.ShowInfoAsync(ex.Message, "Error");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to add {ItemLabelSingular} due to invalid argument", LabelProvider.Instance.ItemLabelSingular);
                await _dialogService.ShowInfoAsync(ex.Message, "Error");
            }
        }

        async Task EditToolAsync(CancellationToken cancellationToken)
        {
            if (SelectedItem == null) return;

            var clone = new ItemModel
            {
                ToolID = SelectedItem.ToolID,
                ToolNumber = SelectedItem.ToolNumber,
                PartNumber = SelectedItem.PartNumber,
                NameDescription = SelectedItem.NameDescription,
                Brand = SelectedItem.Brand,
                Location = SelectedItem.Location,
                QuantityOnHand = SelectedItem.QuantityOnHand,
                RentedQuantity = SelectedItem.RentedQuantity,
                Supplier = SelectedItem.Supplier,
                PurchasedDate = SelectedItem.PurchasedDate,
                Notes = SelectedItem.Notes,
                Keywords = SelectedItem.Keywords,
                IsPowerTool = SelectedItem.IsPowerTool,
                IsCheckedOut = SelectedItem.IsCheckedOut,
                CheckedOutBy = SelectedItem.CheckedOutBy,
                CheckedOutTime = SelectedItem.CheckedOutTime,
                ToolImagePath = SelectedItem.ToolImagePath
            };

            var updated = await _dialogService.ShowEditToolDialogAsync(clone);
            if (updated == null) return;

            try
            {
                await _itemService.UpdateToolAsync(updated, cancellationToken);
                await LoadToolsAsync();
                await FilterToolsAsync(cancellationToken);
                SelectedItem = Tools.FirstOrDefault(t => t.ToolID == updated.ToolID);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync($"You are not authorized to update {LabelProvider.Instance.ItemLabelPlural.ToLower()}.", "Unauthorized");
            }
        }

        void ViewDetails()
        {
            if (SelectedItem == null) return;
            _dialogService.ShowToolDetails(SelectedItem);
        }

        async Task OpenRentalHistoryAsync()
        {
            if (SelectedItem == null) return;
            try
            {
                var history = await _rentalService.GetRentalHistoryForToolAsync(SelectedItem.ToolID);
                _dialogService.ShowRentalHistory(SelectedItem, history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open rental history for {ItemLabelSingular} {ToolID}", LabelProvider.Instance.ItemLabelSingular, SelectedItem.ToolID);
            }
        }

        async Task DeleteToolAsync(CancellationToken cancellationToken)
        {
            if (SelectedItem == null) return;
            var confirm = await _dialogService.ShowConfirmationAsync(
                $"Delete {LabelProvider.Instance.ItemLabelSingular.ToLower()} '{SelectedItem.NameDescription}'?",
                "Confirm Delete");
            if (!confirm)
                return;

            try
            {
                await _itemService.DeleteToolAsync(SelectedItem.ToolID, cancellationToken);
                await LoadToolsAsync();
                await FilterToolsAsync(cancellationToken);
                SelectedItem = null;
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync($"You are not authorized to delete {LabelProvider.Instance.ItemLabelPlural.ToLower()}.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete {ItemLabelSingular} {ToolID}", LabelProvider.Instance.ItemLabelSingular, SelectedItem.ToolID);
                await _dialogService.ShowInfoAsync($"Failed to delete {LabelProvider.Instance.ItemLabelSingular.ToLower()}: {ex.Message}", "Error");
            }
        }

        async Task OpenRentalsAsync(CancellationToken cancellationToken)
        {
            if (SelectedItem == null) return;

            try
            {
                var customers = await _customerService.GetAllCustomersAsync(cancellationToken);
                var result = _dialogService.ShowRentToolDialog(SelectedItem, customers);
                if (result != null)
                {
                    var (customer, dueDate) = result.Value;
                    await _rentalService.RentToolAsync(SelectedItem.ToolID,
                        customer.CustomerID,
                        DateTime.Today,
                        dueDate);
                    await LoadToolsAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync($"You are not authorized to rent {LabelProvider.Instance.ItemLabelPlural.ToLower()}.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rent {ItemLabelSingular} {ToolID}", LabelProvider.Instance.ItemLabelSingular, SelectedItem?.ToolID);
                await _dialogService.ShowInfoAsync($"Failed to rent {LabelProvider.Instance.ItemLabelSingular.ToLower()}: {ex.Message}", "Error");
            }
        }

        void LoadCategories(IEnumerable<ItemModel> tools, bool suppressSearch = false)
        {
            var categories = tools.Select(t => t.Brand)
                                   .Where(b => !string.IsNullOrWhiteSpace(b))
                                   .Distinct()
                                   .OrderBy(b => b)
                                   .ToList();
            categories.Insert(0, "All");
            Categories.ReplaceRange(categories);

            if (!Categories.Contains(SelectedCategory))
            {
                if (suppressSearch)
                {
                    _selectedCategory = "All";
                    OnPropertyChanged(nameof(SelectedCategory));
                }
                else
                {
                    SelectedCategory = "All";
                }
            }
        }

        void Tools_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_suppressToolsChanged)
                return;

            LoadCategories(Tools);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            _searchDebounceTimer.Tick -= OnSearchDebounceTimerTick;
            _searchDebounceTimer.Stop();

            var cts = Interlocked.Exchange(ref _searchCts, null!);
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
            cts?.Dispose();

            Tools.CollectionChanged -= Tools_CollectionChanged;
        }
    }
}
