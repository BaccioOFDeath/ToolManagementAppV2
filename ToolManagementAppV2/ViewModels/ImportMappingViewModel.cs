// File: ViewModels/ImportMappingViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ToolManagementAppV2.ViewModels
{
    public class FieldMapping : ObservableObject
    {
        public string PropertyName { get; }
        public IReadOnlyList<string> AvailableColumns { get; }

        private string _selectedColumn;
        public string SelectedColumn
        {
            get => _selectedColumn;
            set => SetProperty(ref _selectedColumn, value);
        }

        public FieldMapping(string propertyName, IReadOnlyList<string> availableColumns)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            AvailableColumns = availableColumns ?? throw new ArgumentNullException(nameof(availableColumns));
            _selectedColumn = AvailableColumns.FirstOrDefault();
        }
    }

    public class ImportMappingViewModel : ObservableObject
    {
        public IReadOnlyList<string> ColumnHeaders { get; }
        public ObservableCollection<FieldMapping> Mappings { get; }

        public ImportMappingViewModel(IEnumerable<string> headers, IEnumerable<string> properties)
        {
            var headerList = (headers ?? Enumerable.Empty<string>()).ToList();
            ColumnHeaders = headerList;

            Mappings = new ObservableCollection<FieldMapping>(
                (properties ?? Enumerable.Empty<string>())
                    .Select(prop => new FieldMapping(prop, ColumnHeaders))
            );
        }
    }
}
