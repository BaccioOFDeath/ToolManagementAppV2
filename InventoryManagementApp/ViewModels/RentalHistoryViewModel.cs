using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.ViewModels.Rental
{
    public class RentalHistoryViewModel : ObservableObject
    {
        private readonly List<RentalModel> _allHistory;
        private readonly ILogger<RentalHistoryViewModel> _logger;
        private readonly IDialogService _dialogService;

        public ObservableCollection<RentalModel> History { get; }
        public string ItemDisplayName { get; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private RentalModel _selectedEntry;
        public RentalModel SelectedEntry
        {
            get => _selectedEntry;
            set => SetProperty(ref _selectedEntry, value);
        }

        public IRelayCommand SearchCommand { get; }
        public IRelayCommand ExportCsvCommand { get; }
        public IRelayCommand CloseCommand { get; }

        public RentalHistoryViewModel(ItemModel? item, IEnumerable<RentalModel>? history, IDialogService dialogService, ILogger<RentalHistoryViewModel>? logger = null)
        {
            ItemDisplayName = item != null
                ? $"{item.ItemNumber} - {item.NameDescription}"
                : "Rental History";

            _allHistory = (history ?? Enumerable.Empty<RentalModel>()).ToList();
            History = new ObservableCollection<RentalModel>(_allHistory);
            _logger = logger ?? NullLogger<RentalHistoryViewModel>.Instance;
            _dialogService = dialogService;

            SearchCommand = new RelayCommand(ExecuteSearch);
            ExportCsvCommand = new RelayCommand(ExportCsv);
            CloseCommand = new RelayCommand(CloseWindow);
        }

        void ExecuteSearch()
        {
            var term = string.IsNullOrWhiteSpace(SearchText) ? string.Empty : SearchText.Trim();
            IEnumerable<RentalModel> results = _allHistory;
            if (!string.IsNullOrEmpty(term))
            {
                results = _allHistory.Where(r =>
                    r.RentalID.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (r.ItemNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.CustomerName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.Status?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            History.ReplaceRange(results);
        }

        void ExportCsv()
        {
            string? path = null;

            if (System.Windows.Application.Current != null)
            {
                try
                {
                    var dlg = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "CSV Files|*.csv",
                        FileName = "rental_history.csv"
                    };
                    if (dlg.ShowDialog() == true)
                        path = dlg.FileName;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to show save file dialog for rental history export");
                }
            }

            path ??= Path.Combine(Environment.CurrentDirectory, "rental_history.csv");

            var sb = new StringBuilder();
            sb.AppendLine("RentalID,ItemNumber,CustomerName,RentalDate,DueDate,ReturnDate,Status");
            foreach (var r in History)
            {
                sb.AppendLine(string.Join(',',
                    r.RentalID,
                    Escape(r.ItemNumber),
                    Escape(r.CustomerName),
                    r.RentalDate.ToString("o"),
                    r.DueDate.ToString("o"),
                    r.ReturnDate?.ToString("o") ?? string.Empty,
                    Escape(r.Status)));
            }

            try
            {
                File.WriteAllText(path, sb.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export rental history to {Path}", path);
                _dialogService.ShowInfo($"Failed to export rental history: {ex.Message}", "Error");
            }
        }

        static string Escape(string? value) =>
            value?.Replace("\"", "\"\"") ?? string.Empty;

        void CloseWindow()
        {
            if (System.Windows.Application.Current == null) return;
            var window = System.Windows.Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }
    }
}
