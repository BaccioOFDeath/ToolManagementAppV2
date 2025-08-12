using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.ViewModels.Rental;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.ViewModels
{
    public class ToolManagementViewModel : ObservableObject
    {
        private readonly IToolService _toolService;
        private readonly ICustomerService _customerService;
        private readonly IRentalService _rentalService;

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
                    ((RelayCommand)OpenRentalsCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)EditToolCommand).NotifyCanExecuteChanged();
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

        public IRelayCommand SearchCommand { get; }
        public IRelayCommand NewToolCommand { get; }
        public IRelayCommand EditToolCommand { get; }
        public IRelayCommand DeleteToolCommand { get; }
        public IRelayCommand OpenRentalsCommand { get; }
        public IRelayCommand ViewDetailsCommand { get; }

        public Func<ToolModel, ToolModel?> EditToolDialog { get; set; }
        public Action<ToolModel>? ViewDetailsDialog { get; set; }

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
                    SearchCommand.Execute(null);
                }
            }
        }

        public ToolManagementViewModel(IToolService toolService,
                                       ICustomerService customerService,
                                       IRentalService rentalService)
        {
            _toolService = toolService;
            SearchCommand = new RelayCommand(FilterTools);
            NewToolCommand = new RelayCommand(AddTool);
            _customerService = customerService;
            EditToolCommand = new RelayCommand(EditTool, () => SelectedTool != null);
            DeleteToolCommand = new RelayCommand(DeleteTool);
            OpenRentalsCommand = new RelayCommand(OpenRentals, () => SelectedTool != null);
            ViewDetailsCommand = new RelayCommand(ViewDetails, () => SelectedTool != null);
            EditToolDialog = DefaultEditToolDialog;
            ViewDetailsDialog = DefaultViewDetailsDialog;
        }

        public void LoadTools()
        {
            var all = _toolService.GetAllTools();
            Tools.ReplaceRange(all);
            SearchResults.ReplaceRange(all);
            CategorizeTools(all);
            LoadCategories(all);
        }

        /// <summary>
        /// Applies text and category filters to the tool list.
        /// Invoked by <see cref="SearchCommand"/> whenever the search text or
        /// <see cref="SelectedCategory"/> changes and recomputes <see cref="Categories"/>.
        /// </summary>
        void FilterTools()
        {
            var term = string.IsNullOrWhiteSpace(SearchTerm) ? string.Empty : SearchTerm.Trim();
            var all = _toolService.GetAllTools();
            LoadCategories(all);
            IEnumerable<ToolModel> results = string.IsNullOrEmpty(term)
                ? all
                : _toolService.SearchTools(term);
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                results = results.Where(t => string.Equals(t.Brand, SelectedCategory, StringComparison.OrdinalIgnoreCase));
            }
            SearchResults.ReplaceRange(results);
            CategorizeTools(results);
        }

        void AddTool()
        {
            _toolService.AddTool(NewTool);
            LoadTools();
            FilterTools();
            NewTool = new ToolModel();
        }

        void EditTool()
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
                Supplier = SelectedTool.Supplier,
                Notes = SelectedTool.Notes
            };

            var updated = EditToolDialog?.Invoke(clone);
            if (updated == null) return;

            _toolService.UpdateTool(updated);
            LoadTools();
            FilterTools();
            SelectedTool = Tools.FirstOrDefault(t => t.ToolID == updated.ToolID);
        }

        void ViewDetails()
        {
            if (SelectedTool == null) return;
            ViewDetailsDialog?.Invoke(SelectedTool);
        }

        void DeleteTool()
        {
            if (SelectedTool == null) return;
            _toolService.DeleteTool(SelectedTool.ToolID);
            LoadTools();
            FilterTools();
            SelectedTool = null;
        }

        void OpenRentals()
        {
            if (SelectedTool == null) return;

            var customers = _customerService.GetAllCustomers();
            var vm = new RentToolPopupViewModel(SelectedTool, customers);
            var win = new RentToolPopupWindow { DataContext = vm };
            vm.RequestClose += (_, _) => win.Close();
            win.ShowDialog();

            if (vm.SelectedCustomerResult != null)
            {
                _rentalService.RentTool(SelectedTool.ToolID,
                    vm.SelectedCustomerResult.CustomerID,
                    DateTime.Today,
                    vm.SelectedDueDateResult);
                LoadTools();
            }
        }

        void CategorizeTools(IEnumerable<ToolModel> tools)
        {
            HandTools.ReplaceRange(tools.Where(t => !IsPowerTool(t)));
            PowerTools.ReplaceRange(tools.Where(IsPowerTool));
        }

        static bool IsPowerTool(ToolModel tool) =>
            tool?.NameDescription?.Contains("power", StringComparison.OrdinalIgnoreCase) == true ||
            tool?.NameDescription?.Contains("cordless", StringComparison.OrdinalIgnoreCase) == true ||
            tool?.NameDescription?.Contains("electric", StringComparison.OrdinalIgnoreCase) == true ||
            tool?.NameDescription?.Contains("drill", StringComparison.OrdinalIgnoreCase) == true;

        void LoadCategories(IEnumerable<ToolModel> tools)
        {
            var categories = tools.Select(t => t.Brand)
                                   .Where(b => !string.IsNullOrWhiteSpace(b))
                                   .Distinct()
                                   .OrderBy(b => b)
                                   .ToList();
            categories.Insert(0, "All");
            Categories.ReplaceRange(categories);
        }

        ToolModel? DefaultEditToolDialog(ToolModel tool)
        {
            ToolEditWindow win = null!;
            win = new ToolEditWindow(tool,
                onSave: () => win.DialogResult = true,
                onCancel: () => win.DialogResult = false);
            try { win.Owner = System.Windows.Application.Current?.MainWindow; } catch { }
            try { return win.ShowDialog() == true ? tool : null; } catch { return null; }
        }

        void DefaultViewDetailsDialog(ToolModel tool)
        {
            ToolDetailsWindow win = null!;
            win = new ToolDetailsWindow(tool);
            try { win.Owner = System.Windows.Application.Current?.MainWindow; } catch { }
            try { win.ShowDialog(); } catch { }
        }
    }
}
