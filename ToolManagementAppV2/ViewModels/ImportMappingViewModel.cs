// File: ViewModels/ImportMappingViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ToolManagementAppV2.ViewModels
{
    public class FieldMapping : ObservableObject
    {
        public string PropertyName { get; }
        private string _selectedColumn;
        public string SelectedColumn
        {
            get => _selectedColumn;
            set => SetProperty(ref _selectedColumn, value);
        }
        public FieldMapping(string name) => PropertyName = name;
    }

    public class ImportMappingViewModel : ObservableObject
    {
        public List<string> ColumnHeaders { get; }
        public ObservableCollection<FieldMapping> Mappings { get; }

        public ImportMappingViewModel(IEnumerable<string> headers, IEnumerable<string> properties)
        {
            ColumnHeaders = headers.ToList();
            Mappings = new ObservableCollection<FieldMapping>(
                               properties.Select(p => new FieldMapping(p)));
        }
    }
}
