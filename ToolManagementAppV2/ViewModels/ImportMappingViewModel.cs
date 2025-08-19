using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ToolManagementAppV2.ViewModels
{
    public class FieldMapping : ObservableObject
    {
        public string PropertyName { get; }
        public IReadOnlyList<string> AvailableColumns { get; }

        private string? _selectedColumn;
        public string? SelectedColumn
        {
            get => _selectedColumn;
            set => SetProperty(ref _selectedColumn, value);
        }

        public FieldMapping(string propertyName, IReadOnlyList<string> availableColumns)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            AvailableColumns = availableColumns ?? throw new ArgumentNullException(nameof(availableColumns));
            _selectedColumn = null;
        }
    }

    public class ImportMappingViewModel : ObservableObject
    {
        public IReadOnlyList<string> ColumnHeaders { get; }
        public ObservableCollection<FieldMapping> Mappings { get; }

        public IRelayCommand OkCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public ImportMappingViewModel(
            IEnumerable<string> headers,
            IEnumerable<string> properties,
            Action onOk,
            Action onCancel)
        {
            var headerList = (headers ?? Enumerable.Empty<string>()).ToList();
            ColumnHeaders = headerList;

            Mappings = new ObservableCollection<FieldMapping>(
                (properties ?? Enumerable.Empty<string>())
                    .Select(prop => new FieldMapping(prop, ColumnHeaders))
            );

            OkCommand = new RelayCommand(() =>
            {
                if (Mappings.Any(m => string.IsNullOrEmpty(m.SelectedColumn)))
                    return;
                onOk();
            });

            CancelCommand = new RelayCommand(onCancel);
        }
    }
}
