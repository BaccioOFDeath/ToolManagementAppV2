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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.ViewModels
{
    public class ToolManagementViewModel : ObservableObject, IDisposable
    {
        private readonly IToolService _toolService;
        private readonly ICustomerService _customerService;
        private readonly IRentalService _rentalService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ToolManagementViewModel> _logger;

        public ObservableCollection<ToolModel> Tools { get; } = new();
        public ObservableCollection<ToolModel> SearchResults { get; } = new();
        public ObservableCollection<ToolModel> HandTools { get; } = new();
        public ObservableCollection<ToolModel> PowerTools { get; } = new();

        /// <summary>
        /// List of available tool categories derived from distinct brands
        /// in the current tool set; rebuilt whenever tools are loaded or filtered.
        /// </summary>
        public ObservableCollection<string> Categories { get; } = new();

        private ToolModel _newTool = new();
        public ToolModel NewTool
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

        private ToolModel _selectedTool;
        public ToolModel SelectedTool
        {
            get => _selectedTool;
            set
            {
                if (SetProperty(ref _selectedTool, value))
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

        public ToolManagementViewModel(IToolService toolService,
                                       ICustomerService customerService,
                                       IRentalService rentalService,
                                       IDialogService dialogService,
                                       ILogger<ToolManagementViewModel>? logger = null,
                                       IDispatcherTimer? searchDebounceTimer = null)
        {
            _toolService = toolService;
            _customerService = customerService;
            _rentalService = rentalService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<ToolManagementViewModel>.Instance;
            SearchCommand = new AsyncRelayCommand<CancellationToken>(FilterToolsAsync);
            _searchDebounceTimer = searchDebounceTimer ?? new DispatcherTimerWrapper { Interval = TimeSpan.FromMilliseconds(300) };
            _searchDebounceTimer.Tick += OnSearchDebounceTimerTick;
            NewToolCommand = new AsyncRelayCommand(ct => AddToolAsync(ct));
            EditToolCommand = new AsyncRelayCommand(ct => EditToolAsync(ct), () => SelectedTool != null);
            DeleteToolCommand = new AsyncRelayCommand(ct => DeleteToolAsync(ct));
            OpenRentalsCommand = new AsyncRelayCommand(ct => OpenRentalsAsync(ct), () => SelectedTool != null);
            ViewDetailsCommand = new RelayCommand(ViewDetails, () => SelectedTool != null);
            OpenRentalHistoryCommand = new AsyncRelayCommand(OpenRentalHistoryAsync, () => SelectedTool != null);
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

        bool _suppressToolsChanged;

        public async Task LoadToolsAsync()
        {
            _suppressToolsChanged = true;
            try
            {
                var all = await _toolService.GetAllToolsAsync();
                Tools.ReplaceRange(all);
                SearchResults.ReplaceRange(all);
                CategorizeTools(all);
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
            IEnumerable<ToolModel> source;

            if (!string.IsNullOrEmpty(term))
            {
                source = await _toolService.SearchToolsAsync(term, cancellationToken);
            }
            else
            {
                if (Tools.Count == 0)
                {
                    var all = await _toolService.GetAllToolsAsync();
                    Tools.ReplaceRange(all);
                }
                source = Tools;
            }

            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                source = source.Where(t => string.Equals(t.Brand, SelectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            SearchResults.ReplaceRange(source);
            CategorizeTools(source);
            LoadCategories(source, suppressSearch: true);
        }

        async Task AddToolAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _toolService.AddToolAsync(NewTool, cancellationToken);
                await LoadToolsAsync();
                await FilterToolsAsync(cancellationToken);
                NewTool = new ToolModel();
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to add tools.", "Unauthorized");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to add tool due to invalid operation");
                await _dialogService.ShowInfoAsync(ex.Message, "Error");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to add tool due to invalid argument");
                await _dialogService.ShowInfoAsync(ex.Message, "Error");
            }
        }

        async Task EditToolAsync(CancellationToken cancellationToken)
        {
            if (SelectedTool == null) return;

            var clone = new ToolModel
            {
                ToolID = SelectedTool.ToolID,
                ToolNumber = SelectedTool.ToolNumber,
                PartNumber = SelectedTool.PartNumber,
                NameDescription = SelectedTool.NameDescription,
                Brand = SelectedTool.Brand,
                Location = SelectedTool.Location,
                QuantityOnHand = SelectedTool.QuantityOnHand,
                RentedQuantity = SelectedTool.RentedQuantity,
                Supplier = SelectedTool.Supplier,
                PurchasedDate = SelectedTool.PurchasedDate,
                Notes = SelectedTool.Notes,
                Keywords = SelectedTool.Keywords,
                IsPowerTool = SelectedTool.IsPowerTool,
                IsCheckedOut = SelectedTool.IsCheckedOut,
                CheckedOutBy = SelectedTool.CheckedOutBy,
                CheckedOutTime = SelectedTool.CheckedOutTime,
                ToolImagePath = SelectedTool.ToolImagePath
            };

            var updated = await _dialogService.ShowEditToolDialogAsync(clone);
            if (updated == null) return;

            try
            {
                await _toolService.UpdateToolAsync(updated, cancellationToken);
                await LoadToolsAsync();
                await FilterToolsAsync(cancellationToken);
                SelectedTool = Tools.FirstOrDefault(t => t.ToolID == updated.ToolID);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to update tools.", "Unauthorized");
            }
        }

        void ViewDetails()
        {
            if (SelectedTool == null) return;
            _dialogService.ShowToolDetails(SelectedTool);
        }

        async Task OpenRentalHistoryAsync()
        {
            if (SelectedTool == null) return;
            try
            {
                var history = await _rentalService.GetRentalHistoryForToolAsync(SelectedTool.ToolID);
                _dialogService.ShowRentalHistory(SelectedTool, history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open rental history for tool {ToolID}", SelectedTool.ToolID);
            }
        }

        async Task DeleteToolAsync(CancellationToken cancellationToken)
        {
            if (SelectedTool == null) return;
            var confirm = await _dialogService.ShowConfirmationAsync(
                $"Delete tool '{SelectedTool.NameDescription}'?",
                "Confirm Delete");
            if (!confirm)
                return;

            try
            {
                await _toolService.DeleteToolAsync(SelectedTool.ToolID, cancellationToken);
                await LoadToolsAsync();
                await FilterToolsAsync(cancellationToken);
                SelectedTool = null;
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to delete tools.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete tool {ToolID}", SelectedTool.ToolID);
                await _dialogService.ShowInfoAsync($"Failed to delete tool: {ex.Message}", "Error");
            }
        }

        async Task OpenRentalsAsync(CancellationToken cancellationToken)
        {
            if (SelectedTool == null) return;

            try
            {
                var customers = await _customerService.GetAllCustomersAsync(cancellationToken);
                var result = _dialogService.ShowRentToolDialog(SelectedTool, customers);
                if (result != null)
                {
                    var (customer, dueDate) = result.Value;
                    await _rentalService.RentToolAsync(SelectedTool.ToolID,
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
                await _dialogService.ShowInfoAsync("You are not authorized to rent tools.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rent tool {ToolID}", SelectedTool?.ToolID);
                await _dialogService.ShowInfoAsync($"Failed to rent tool: {ex.Message}", "Error");
            }
        }

        void CategorizeTools(IEnumerable<ToolModel> tools)
        {
            HandTools.ReplaceRange(tools.Where(t => !IsPowerTool(t)));
            PowerTools.ReplaceRange(tools.Where(IsPowerTool));
        }

        static bool IsPowerTool(ToolModel tool) => tool?.IsPowerTool == true;

        void LoadCategories(IEnumerable<ToolModel> tools, bool suppressSearch = false)
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
            _searchDebounceTimer.Tick -= OnSearchDebounceTimerTick;
            _searchDebounceTimer.Stop();
            _searchCts.Cancel();
            _searchCts.Dispose();
            Tools.CollectionChanged -= Tools_CollectionChanged;
        }
    }
}
