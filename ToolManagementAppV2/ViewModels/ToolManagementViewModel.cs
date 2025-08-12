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
                    ((RelayCommand)OpenRentalsCommand).NotifyCanExecuteChanged();
            }
        }

        public IRelayCommand SearchCommand { get; }
        public IRelayCommand NewToolCommand { get; }
        public IRelayCommand UpdateToolCommand { get; }
        public IRelayCommand DeleteToolCommand { get; }
        public IRelayCommand OpenRentalsCommand { get; }

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
            _customerService = customerService;
            _rentalService = rentalService;
            SearchCommand = new RelayCommand(SearchTools);
            NewToolCommand = new RelayCommand(AddTool);
            UpdateToolCommand = new RelayCommand(UpdateTool);
            DeleteToolCommand = new RelayCommand(DeleteTool);
            OpenRentalsCommand = new RelayCommand(OpenRentals, () => SelectedTool != null);
        }

        public void LoadTools()
        {
            var all = _toolService.GetAllTools();
            Tools.ReplaceRange(all);
            SearchResults.ReplaceRange(all);
            CategorizeTools(all);
        }

        void SearchTools()
        {
            var term = string.IsNullOrWhiteSpace(SearchTerm) ? string.Empty : SearchTerm.Trim();
            var results = string.IsNullOrEmpty(term)
                ? _toolService.GetAllTools()
                : _toolService.SearchTools(term);
            SearchResults.ReplaceRange(results);
            CategorizeTools(results);
        }

        void AddTool()
        {
            _toolService.AddTool(NewTool);
            LoadTools();
            SearchTools();
            NewTool = new ToolModel();
        }

        void UpdateTool()
        {
            if (SelectedTool == null) return;
            _toolService.UpdateTool(SelectedTool);
            LoadTools();
            SearchTools();
            SelectedTool = null;
        }

        void DeleteTool()
        {
            if (SelectedTool == null) return;
            _toolService.DeleteTool(SelectedTool.ToolID);
            LoadTools();
            SearchTools();
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
    }
}
