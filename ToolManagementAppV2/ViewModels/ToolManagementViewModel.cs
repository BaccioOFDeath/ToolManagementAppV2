using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities;
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.ViewModels
{
    public class ToolManagementViewModel : ObservableObject
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
                    SearchCommand.Execute(null);
                }
            }
        }

        public IAsyncRelayCommand SearchCommand { get; }
        public IAsyncRelayCommand NewToolCommand { get; }
        public IAsyncRelayCommand EditToolCommand { get; }
        public IAsyncRelayCommand DeleteToolCommand { get; }
        public IAsyncRelayCommand OpenRentalsCommand { get; }
        public IRelayCommand ViewDetailsCommand { get; }

        readonly IDispatcherTimer _searchDebounceTimer;

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
            SearchCommand = new AsyncRelayCommand(FilterToolsAsync);
            _searchDebounceTimer = searchDebounceTimer ?? new DispatcherTimerWrapper { Interval = TimeSpan.FromMilliseconds(300) };
            _searchDebounceTimer.Tick += (s, e) =>
            {
                _searchDebounceTimer.Stop();
                SearchCommand.Execute(null);
            };
            NewToolCommand = new AsyncRelayCommand(AddToolAsync);
            EditToolCommand = new AsyncRelayCommand(EditToolAsync, () => SelectedTool != null);
            DeleteToolCommand = new AsyncRelayCommand(DeleteToolAsync);
            OpenRentalsCommand = new AsyncRelayCommand(OpenRentalsAsync, () => SelectedTool != null);
            ViewDetailsCommand = new RelayCommand(ViewDetails, () => SelectedTool != null);
            // Ensure no duplicate event subscriptions when the view model is
            // constructed multiple times or the collection persists across
            // instances.
            Tools.CollectionChanged -= Tools_CollectionChanged;
            Tools.CollectionChanged += Tools_CollectionChanged;
        }

        public async Task LoadToolsAsync()
        {
            var all = await _toolService.GetAllToolsAsync();
            // Temporarily detach to prevent intermediate collection changes
            // from firing the handler and potentially resulting in duplicate
            // subscriptions.
            Tools.CollectionChanged -= Tools_CollectionChanged;
            Tools.ReplaceRange(all);
            SearchResults.ReplaceRange(all);
            CategorizeTools(all);
            LoadCategories(Tools);
            // Remove again in case the handler was reattached during the load
            // process, then attach once to guarantee a single subscription.
            Tools.CollectionChanged -= Tools_CollectionChanged;
            Tools.CollectionChanged += Tools_CollectionChanged;
        }

        /// <summary>
        /// Applies text and category filters to the tool list.
        /// Invoked by <see cref="SearchCommand"/> whenever the search text or
        /// <see cref="SelectedCategory"/> changes and recomputes <see cref="Categories"/>.
        /// </summary>
        async Task FilterToolsAsync()
        {
            var term = string.IsNullOrWhiteSpace(SearchTerm) ? string.Empty : SearchTerm.Trim();
            IEnumerable<ToolModel> source;

            if (!string.IsNullOrEmpty(term))
            {
                source = await _toolService.SearchToolsAsync(term);
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

        async Task AddToolAsync()
        {
            try
            {
                await _toolService.AddToolAsync(NewTool);
                await LoadToolsAsync();
                await FilterToolsAsync();
                NewTool = new ToolModel();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to add tool due to invalid operation");
                _dialogService.ShowInfo(ex.Message, "Error");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to add tool due to invalid argument");
                _dialogService.ShowInfo(ex.Message, "Error");
            }
        }

        async Task EditToolAsync()
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

            var updated = _dialogService.ShowEditToolDialog(clone);
            if (updated == null) return;

            await _toolService.UpdateToolAsync(updated);
            await LoadToolsAsync();
            await FilterToolsAsync();
            SelectedTool = Tools.FirstOrDefault(t => t.ToolID == updated.ToolID);
        }

        void ViewDetails()
        {
            if (SelectedTool == null) return;
            _dialogService.ShowToolDetails(SelectedTool);
        }

        async Task DeleteToolAsync()
        {
            if (SelectedTool == null) return;
            var confirm = _dialogService.ShowConfirmation(
                $"Delete tool '{SelectedTool.NameDescription}'?",
                "Confirm Delete");
            if (!confirm)
                return;

            try
            {
                await _toolService.DeleteToolAsync(SelectedTool.ToolID);
                await LoadToolsAsync();
                await FilterToolsAsync();
                SelectedTool = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete tool {ToolID}", SelectedTool.ToolID);
                _dialogService.ShowInfo($"Failed to delete tool: {ex.Message}", "Error");
            }
        }

        async Task OpenRentalsAsync()
        {
            if (SelectedTool == null) return;

            var customers = await _customerService.GetAllCustomersAsync();
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

        void Tools_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => LoadCategories(Tools);

    }
}
