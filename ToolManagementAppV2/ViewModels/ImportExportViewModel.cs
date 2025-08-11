using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.ViewModels
{
    public class ImportExportViewModel : ObservableObject
    {
        private readonly IToolService _toolService;
        private readonly IFileDialogService _fileDialogService;

        public IRelayCommand ImportCommand { get; }
        public IRelayCommand ExportCommand { get; }

        public ImportExportViewModel(IToolService toolService, IFileDialogService fileDialogService)
        {
            _toolService = toolService;
            _fileDialogService = fileDialogService;
            ImportCommand = new RelayCommand(ImportTools);
            ExportCommand = new RelayCommand(ExportTools);
        }

        void ImportTools()
        {
            var path = _fileDialogService.OpenFile("CSV Files|*.csv");
            if (!string.IsNullOrEmpty(path))
                _toolService.ImportToolsFromCsv(path, new Dictionary<string, string>());
        }

        void ExportTools()
        {
            var path = _fileDialogService.OpenFile("CSV Files|*.csv");
            if (!string.IsNullOrEmpty(path))
                _toolService.ExportToolsToCsv(path);
        }
    }
}
