using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using WpfDataGrid = System.Windows.Controls.DataGrid;
using WpfMessageBox = System.Windows.MessageBox;
using WpfPrintDialog = System.Windows.Controls.PrintDialog;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ItemSearchPage : Page
    {
        private readonly ObservableCollection<SearchHistoryEntry> _searchHistory = new();
        private readonly ObservableCollection<UnavailableDemandEntry> _unavailableDemand = new();
        private readonly Dictionary<int, UnavailableDemandEntry> _demandByItemId = new();
        private ItemManagementViewModel? _attachedViewModel;
        private bool _recordSearchPending;
        private string _lastSearchSignature = string.Empty;

        public ItemSearchPage()
        {
            InitializeComponent();
            RecentSearchGrid.ItemsSource = _searchHistory;
            UnavailableDemandGrid.ItemsSource = _unavailableDemand;
            UpdateSearchIntelligenceSummary();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateState();
            if (DataContext is ItemManagementViewModel vm)
            {
                AttachViewModel(vm);
                vm.SelectedCategory = "All";
                await vm.SearchCommand.ExecuteAsync(null);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            DetachViewModel();
        }

        private void AttachViewModel(ItemManagementViewModel vm)
        {
            if (ReferenceEquals(_attachedViewModel, vm))
                return;

            DetachViewModel();
            _attachedViewModel = vm;
            vm.SearchResults.CollectionChanged += SearchResults_CollectionChanged;
        }

        private void DetachViewModel()
        {
            if (_attachedViewModel != null)
                _attachedViewModel.SearchResults.CollectionChanged -= SearchResults_CollectionChanged;
            _attachedViewModel = null;
        }

        private void UpdateState()
        {
            VisualStateManager.GoToState(this, "Wide", true);
        }

        private void ItemGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is not ItemManagementViewModel vm || sender is not WpfDataGrid grid || grid.SelectedItem is not ItemModel item)
                return;

            vm.SelectedItem = item;
            if (vm.ViewDetailsCommand.CanExecute(null))
                vm.ViewDetailsCommand.Execute(null);
        }

        private void SearchResults_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_recordSearchPending)
                return;

            _recordSearchPending = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _recordSearchPending = false;
                RecordCurrentSearch();
            }), DispatcherPriority.Background);
        }

        private void RecordCurrentSearch()
        {
            if (_attachedViewModel == null)
                return;

            var term = (_attachedViewModel.SearchText ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(term))
            {
                UpdateSearchIntelligenceSummary();
                return;
            }

            var category = string.IsNullOrWhiteSpace(_attachedViewModel.SelectedCategory)
                ? "All"
                : _attachedViewModel.SelectedCategory;
            var results = _attachedViewModel.SearchResults.ToList();
            var unavailable = results.Where(IsUnavailable).ToList();
            var signature = BuildSearchSignature(term, category, results, unavailable);
            if (string.Equals(signature, _lastSearchSignature, StringComparison.Ordinal))
                return;

            _lastSearchSignature = signature;
            UpsertSearchHistory(term, category, results.Count, unavailable.Count);
            CaptureUnavailableDemand(term, unavailable);
            UpdateSearchIntelligenceSummary();
        }

        private static string BuildSearchSignature(string term, string category, IEnumerable<ItemModel> results, IEnumerable<ItemModel> unavailable)
        {
            var resultIds = string.Join(",", results.Select(item => item.ItemID).OrderBy(id => id));
            var unavailableIds = string.Join(",", unavailable.Select(item => item.ItemID).OrderBy(id => id));
            return $"{term}|{category}|{resultIds}|{unavailableIds}";
        }

        private void UpsertSearchHistory(string term, string category, int resultCount, int unavailableCount)
        {
            var existing = _searchHistory.FirstOrDefault(entry =>
                string.Equals(entry.Term, term, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(entry.Category, category, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
                _searchHistory.Remove(existing);
            else
                existing = new SearchHistoryEntry();

            existing.Term = term;
            existing.Category = category;
            existing.ResultCount = resultCount;
            existing.UnavailableCount = unavailableCount;
            existing.LastSearched = DateTime.Now;
            _searchHistory.Insert(0, existing);

            while (_searchHistory.Count > 10)
                _searchHistory.RemoveAt(_searchHistory.Count - 1);
        }

        private void CaptureUnavailableDemand(string term, IEnumerable<ItemModel> unavailableItems)
        {
            foreach (var item in unavailableItems.GroupBy(item => item.ItemID).Select(group => group.First()))
            {
                if (!_demandByItemId.TryGetValue(item.ItemID, out var entry))
                {
                    entry = new UnavailableDemandEntry { Item = item };
                    _demandByItemId[item.ItemID] = entry;
                }

                entry.UpdateFrom(item, term);
            }

            _unavailableDemand.Clear();
            foreach (var entry in _demandByItemId.Values
                         .OrderByDescending(entry => entry.HitCount)
                         .ThenByDescending(entry => entry.LastSearched)
                         .ThenBy(entry => entry.Name)
                         .Take(12))
            {
                _unavailableDemand.Add(entry);
            }
        }

        private void UpdateSearchIntelligenceSummary()
        {
            SearchIntelligenceSummaryText.Text = _searchHistory.Count == 0
                ? "No search activity yet"
                : $"{_searchHistory.Count} recent searches; {_unavailableDemand.Count} unavailable demand signals";
        }

        private void RepeatSelectedSearch_Click(object sender, RoutedEventArgs e)
        {
            RepeatSearch(RecentSearchGrid.SelectedItem as SearchHistoryEntry);
        }

        private void RecentSearchGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RepeatSearch(RecentSearchGrid.SelectedItem as SearchHistoryEntry);
        }

        private void RepeatSearch(SearchHistoryEntry? entry)
        {
            if (entry == null || DataContext is not ItemManagementViewModel vm)
                return;

            if (vm.Categories.Contains(entry.Category))
                vm.SelectedCategory = entry.Category;
            vm.SearchText = entry.Term;
            _ = vm.SearchCommand.ExecuteAsync(null);
        }

        private void OpenDemandItem_Click(object sender, RoutedEventArgs e)
        {
            OpenDemandItem(UnavailableDemandGrid.SelectedItem as UnavailableDemandEntry);
        }

        private void UnavailableDemandGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenDemandItem(UnavailableDemandGrid.SelectedItem as UnavailableDemandEntry);
        }

        private void OpenDemandItem(UnavailableDemandEntry? entry)
        {
            if (entry?.Item == null || DataContext is not ItemManagementViewModel vm)
                return;

            vm.SelectedItem = entry.Item;
            if (vm.ViewDetailsCommand.CanExecute(null))
                vm.ViewDetailsCommand.Execute(null);
        }

        private void ClearSearchIntelligence_Click(object sender, RoutedEventArgs e)
        {
            if (_searchHistory.Count == 0 && _unavailableDemand.Count == 0)
                return;

            _searchHistory.Clear();
            _unavailableDemand.Clear();
            _demandByItemId.Clear();
            _lastSearchSignature = string.Empty;
            UpdateSearchIntelligenceSummary();
        }

        private void PrintSearchIntelligence_Click(object sender, RoutedEventArgs e)
        {
            if (_searchHistory.Count == 0 && _unavailableDemand.Count == 0)
            {
                WpfMessageBox.Show("There is no search intelligence to print yet.", "Print", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var printDialog = new WpfPrintDialog();
            if (printDialog.ShowDialog() != true)
                return;

            var document = BuildSearchIntelligenceDocument(_searchHistory.ToList(), _unavailableDemand.ToList());
            document.PageWidth = printDialog.PrintableAreaWidth;
            document.PageHeight = printDialog.PrintableAreaHeight;
            document.PagePadding = new Thickness(36);
            document.ColumnWidth = printDialog.PrintableAreaWidth;
            printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "Item Search Intelligence");
        }

        private void PrintSearchResults_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ItemManagementViewModel vm)
                PrintItems("Item Search Results", vm.SearchResults);
        }

        private void PrintCheckedOut_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ItemManagementViewModel vm)
                PrintItems("Currently Checked Out Items", vm.CheckedOutItems);
        }

        private void PrintItems(string title, IEnumerable<ItemModel> items)
        {
            var itemList = items.ToList();
            if (itemList.Count == 0)
            {
                WpfMessageBox.Show("There are no rows to print.", "Print", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var printDialog = new WpfPrintDialog();
            if (printDialog.ShowDialog() != true)
                return;

            var document = BuildPrintDocument(title, itemList);
            document.PageWidth = printDialog.PrintableAreaWidth;
            document.PageHeight = printDialog.PrintableAreaHeight;
            document.PagePadding = new Thickness(36);
            document.ColumnWidth = printDialog.PrintableAreaWidth;
            printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, title);
        }

        private static FlowDocument BuildPrintDocument(string title, IReadOnlyCollection<ItemModel> items)
        {
            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 11
            };

            document.Blocks.Add(new Paragraph(new Run(title))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g} - {items.Count} row(s)"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var table = new Table { CellSpacing = 0 };
            foreach (var width in new[] { 80.0, 180.0, 90.0, 80.0, 70.0, 110.0, 110.0 })
                table.Columns.Add(new TableColumn { Width = new GridLength(width) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);
            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "Item #");
            AddCell(header, "Name");
            AddCell(header, "Status");
            AddCell(header, "Location");
            AddCell(header, "On Hand");
            AddCell(header, "Holder");
            AddCell(header, "Out Since");

            foreach (var item in items)
            {
                var row = new TableRow();
                rowGroup.Rows.Add(row);
                AddCell(row, item.ItemNumber);
                AddCell(row, item.Name);
                AddCell(row, GetStatus(item));
                AddCell(row, item.Location);
                AddCell(row, item.QuantityOnHand.ToString());
                AddCell(row, item.CheckedOutBy);
                AddCell(row, item.CheckedOutTime?.ToString("g") ?? string.Empty);
            }

            document.Blocks.Add(table);
            return document;
        }

        private static FlowDocument BuildSearchIntelligenceDocument(
            IReadOnlyCollection<SearchHistoryEntry> searches,
            IReadOnlyCollection<UnavailableDemandEntry> demand)
        {
            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 11
            };

            document.Blocks.Add(new Paragraph(new Run("Item Search Intelligence"))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g} - {searches.Count} recent search(es), {demand.Count} unavailable demand signal(s)"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 10)
            });

            if (searches.Count > 0)
            {
                AddSectionTitle(document, "Recent Searches");
                var searchTable = new Table { CellSpacing = 0 };
                foreach (var width in new[] { 210.0, 90.0, 70.0, 90.0, 90.0 })
                    searchTable.Columns.Add(new TableColumn { Width = new GridLength(width) });

                var rowGroup = new TableRowGroup();
                searchTable.RowGroups.Add(rowGroup);
                var header = new TableRow { FontWeight = FontWeights.SemiBold };
                rowGroup.Rows.Add(header);
                AddCell(header, "Search");
                AddCell(header, "Brand");
                AddCell(header, "Results");
                AddCell(header, "Unavailable");
                AddCell(header, "Last Search");

                foreach (var entry in searches)
                {
                    var row = new TableRow();
                    rowGroup.Rows.Add(row);
                    AddCell(row, entry.Term);
                    AddCell(row, entry.Category);
                    AddCell(row, entry.ResultCount.ToString());
                    AddCell(row, entry.UnavailableCount.ToString());
                    AddCell(row, entry.LastSearched.ToString("g"));
                }

                document.Blocks.Add(searchTable);
            }

            if (demand.Count > 0)
            {
                AddSectionTitle(document, "Unavailable Demand");
                var demandTable = new Table { CellSpacing = 0 };
                foreach (var width in new[] { 75.0, 150.0, 85.0, 45.0, 90.0, 80.0, 90.0, 120.0 })
                    demandTable.Columns.Add(new TableColumn { Width = new GridLength(width) });

                var rowGroup = new TableRowGroup();
                demandTable.RowGroups.Add(rowGroup);
                var header = new TableRow { FontWeight = FontWeights.SemiBold };
                rowGroup.Rows.Add(header);
                AddCell(header, "Item #");
                AddCell(header, "Item");
                AddCell(header, "Status");
                AddCell(header, "Hits");
                AddCell(header, "Holder");
                AddCell(header, "Location");
                AddCell(header, "Last Search");
                AddCell(header, "Terms");

                foreach (var entry in demand)
                {
                    var row = new TableRow();
                    rowGroup.Rows.Add(row);
                    AddCell(row, entry.ItemNumber);
                    AddCell(row, entry.Name);
                    AddCell(row, entry.Status);
                    AddCell(row, entry.HitCount.ToString());
                    AddCell(row, entry.Holder);
                    AddCell(row, entry.Location);
                    AddCell(row, entry.LastSearched.ToString("g"));
                    AddCell(row, entry.SearchTerms);
                }

                document.Blocks.Add(demandTable);
            }

            return document;
        }

        private static void AddSectionTitle(FlowDocument document, string title)
        {
            document.Blocks.Add(new Paragraph(new Run(title))
            {
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 10, 0, 4)
            });
        }

        private static void AddCell(TableRow row, string text)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(text ?? string.Empty))
            {
                Margin = new Thickness(2)
            })
            {
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 0.5),
                Padding = new Thickness(3, 2, 3, 2)
            });
        }

        private static bool IsUnavailable(ItemModel item)
        {
            return item.HasNoOnHand || item.HasRentedStock || item.IsCheckedOut;
        }

        private static string GetStatus(ItemModel item)
        {
            if (item.IsIncomplete)
                return "Incomplete";
            if (item.IsCheckedOut)
                return "Checked Out";
            if (item.HasRentedStock)
                return "Rented";
            if (item.HasNoOnHand)
                return "Unavailable";
            return "Available";
        }

        public sealed class SearchHistoryEntry
        {
            public string Term { get; set; } = string.Empty;
            public string Category { get; set; } = "All";
            public int ResultCount { get; set; }
            public int UnavailableCount { get; set; }
            public DateTime LastSearched { get; set; }
        }

        public sealed class UnavailableDemandEntry
        {
            private readonly List<string> _terms = new();

            public ItemModel? Item { get; set; }
            public int HitCount { get; private set; }
            public DateTime LastSearched { get; private set; }
            public string ItemNumber => Item?.ItemNumber ?? string.Empty;
            public string Name => Item?.Name ?? string.Empty;
            public string Status => Item == null ? string.Empty : GetStatus(Item);
            public string Holder => Item?.CheckedOutBy ?? string.Empty;
            public string Location => Item?.Location ?? string.Empty;
            public string SearchTerms => string.Join(", ", _terms.Take(3));

            public void UpdateFrom(ItemModel item, string term)
            {
                Item = item;
                HitCount++;
                LastSearched = DateTime.Now;

                if (!_terms.Any(existing => string.Equals(existing, term, StringComparison.OrdinalIgnoreCase)))
                    _terms.Insert(0, term);
                while (_terms.Count > 3)
                    _terms.RemoveAt(_terms.Count - 1);
            }
        }
    }
}
