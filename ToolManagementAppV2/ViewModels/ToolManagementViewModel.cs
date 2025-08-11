using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.ViewModels
{
    public class ToolManagementViewModel : ObservableObject
    {
        private readonly IToolService _toolService;

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

        private string _searchTerm;
        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        private ToolModel _selectedTool;
        public ToolModel SelectedTool
        {
            get => _selectedTool;
            set => SetProperty(ref _selectedTool, value);
        }

        public IRelayCommand SearchCommand { get; }
        public IRelayCommand AddToolCommand { get; }
        public IRelayCommand UpdateToolCommand { get; }
        public IRelayCommand DeleteToolCommand { get; }

        public ToolManagementViewModel(IToolService toolService)
        {
            _toolService = toolService;
            SearchCommand = new RelayCommand(SearchTools);
            AddToolCommand = new RelayCommand(AddTool);
            UpdateToolCommand = new RelayCommand(UpdateTool);
            DeleteToolCommand = new RelayCommand(DeleteTool);
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
            var results = string.IsNullOrWhiteSpace(SearchTerm)
                ? _toolService.GetAllTools()
                : _toolService.SearchTools(SearchTerm);
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
            if (SelectedTool == null)
                return;

            _toolService.UpdateTool(SelectedTool);
            LoadTools();
            SearchTools();
            SelectedTool = null;
        }

        void DeleteTool()
        {
            if (SelectedTool == null)
                return;

            _toolService.DeleteTool(SelectedTool.ToolID);
            LoadTools();
            SearchTools();
            SelectedTool = null;
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
