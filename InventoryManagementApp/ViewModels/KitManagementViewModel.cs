using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Kits;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.ViewModels
{
    public class KitManagementViewModel : ObservableObject
    {
        private readonly KitService _kitService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Kit> Kits { get; }
        public ObservableCollection<Kit> FilteredKits { get; }
        public ObservableCollection<KitItem> KitItems { get; }

        public string KitResultsSummary => $"{FilteredKits.Count} kit{(FilteredKits.Count == 1 ? string.Empty : "s")} shown | {Kits.Count(k => k.IsActive)} active | {Kits.Count(k => !k.IsActive)} inactive";

        public string SelectedKitSummary => SelectedKit == null
            ? "Select a kit to review membership, check availability, copy details, print a pick sheet, or maintain its item list."
            : $"{ValueOrNotRecorded(SelectedKit.KitNumber)} | {ValueOrNotRecorded(SelectedKit.Name)} | {KitItems.Count} item line{(KitItems.Count == 1 ? string.Empty : "s")} | {(SelectedKit.IsActive ? "Active" : "Inactive")}";

        public string SelectedKitDetail => SelectedKit == null
            ? "No kit selected. Choose a row from the directory to see the operational detail here."
            : $"Kit # {ValueOrNotRecorded(SelectedKit.KitNumber)}\nName: {ValueOrNotRecorded(SelectedKit.Name)}\nCategory: {ValueOrNotRecorded(SelectedKit.Category)}\nStatus: {(SelectedKit.IsActive ? "Active" : "Inactive")}\nItems: {KitItems.Count}\nUpdated: {SelectedKit.UpdatedAt:yyyy-MM-dd HH:mm}\n\n{ValueOrNotRecorded(SelectedKit.Description)}";

        public string KitItemsSummary => SelectedKit == null
            ? "No kit selected"
            : $"{KitItems.Count} item line{(KitItems.Count == 1 ? string.Empty : "s")} in {ValueOrNotRecorded(SelectedKit.KitNumber)} | {KitItems.Count(i => !i.IsOptional)} required | {KitItems.Count(i => i.IsOptional)} optional";

        public string SelectedKitItemSummary => SelectedKitItem == null
            ? "Select a kit item to edit quantity, mark optional, or remove it from this kit."
            : $"{ValueOrNotRecorded(SelectedKitItem.ItemNumber)} | {ValueOrNotRecorded(SelectedKitItem.ItemName)} | Qty {SelectedKitItem.Quantity} | {(SelectedKitItem.IsOptional ? "Optional" : "Required")}";

        public string SelectedKitAvailabilitySummary => SelectedKit == null
            ? "Availability check is ready once a kit is selected."
            : "Use Check Availability before promising the kit; required item quantities are checked against current stock.";

        private Kit? _selectedKit;
        public Kit? SelectedKit
        {
            get => _selectedKit;
            set
            {
                if (SetProperty(ref _selectedKit, value))
                {
                    EditKitCommand.NotifyCanExecuteChanged();
                    DeleteKitCommand.NotifyCanExecuteChanged();
                    ViewKitItemsCommand.NotifyCanExecuteChanged();
                    AddKitItemCommand.NotifyCanExecuteChanged();
                    CheckAvailabilityCommand.NotifyCanExecuteChanged();
                    OpenKitDetailsCommand.NotifyCanExecuteChanged();
                    CopySelectedKitCommand.NotifyCanExecuteChanged();
                    PrintSelectedKitCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(SelectedKitSummary));
                    OnPropertyChanged(nameof(SelectedKitDetail));
                    OnPropertyChanged(nameof(KitItemsSummary));
                    OnPropertyChanged(nameof(SelectedKitAvailabilitySummary));

                    if (value != null)
                    {
                        _ = LoadKitItemsAsync(value.KitID);
                    }
                    else
                    {
                        KitItems.Clear();
                        SelectedKitItem = null;
                        RefreshKitItemSummaries();
                    }
                }
            }
        }

        private KitItem? _selectedKitItem;
        public KitItem? SelectedKitItem
        {
            get => _selectedKitItem;
            set
            {
                if (SetProperty(ref _selectedKitItem, value))
                {
                    EditKitItemCommand.NotifyCanExecuteChanged();
                    RemoveKitItemCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(SelectedKitItemSummary));
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        private string _selectedFilter = "Active";
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                {
                    ApplyFilter();
                }
            }
        }

        public ObservableCollection<string> FilterOptions { get; }

        public IAsyncRelayCommand LoadKitsCommand { get; }
        public IAsyncRelayCommand AddKitCommand { get; }
        public IAsyncRelayCommand EditKitCommand { get; }
        public IAsyncRelayCommand DeleteKitCommand { get; }
        public IAsyncRelayCommand ViewKitItemsCommand { get; }
        public IAsyncRelayCommand AddKitItemCommand { get; }
        public IAsyncRelayCommand EditKitItemCommand { get; }
        public IAsyncRelayCommand RemoveKitItemCommand { get; }
        public IAsyncRelayCommand CheckAvailabilityCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }
        public IRelayCommand ClearSearchCommand { get; }
        public IRelayCommand OpenKitDetailsCommand { get; }
        public IRelayCommand CopySelectedKitCommand { get; }
        public IRelayCommand PrintKitListCommand { get; }
        public IRelayCommand PrintSelectedKitCommand { get; }

        public KitManagementViewModel(
            KitService kitService,
            IDialogService dialogService)
        {
            _kitService = kitService;
            _dialogService = dialogService;

            Kits = new ObservableCollection<Kit>();
            FilteredKits = new ObservableCollection<Kit>();
            KitItems = new ObservableCollection<KitItem>();
            FilterOptions = new ObservableCollection<string>
            {
                "All",
                "Active",
                "Inactive"
            };

            LoadKitsCommand = new AsyncRelayCommand(LoadKitsAsync);
            AddKitCommand = new AsyncRelayCommand(AddKitAsync);
            EditKitCommand = new AsyncRelayCommand(EditKitAsync, CanEditOrDelete);
            DeleteKitCommand = new AsyncRelayCommand(DeleteKitAsync, CanEditOrDelete);
            ViewKitItemsCommand = new AsyncRelayCommand(ViewKitItemsAsync, CanEditOrDelete);
            AddKitItemCommand = new AsyncRelayCommand(AddKitItemAsync, CanEditOrDelete);
            EditKitItemCommand = new AsyncRelayCommand(EditKitItemAsync, CanEditOrRemoveKitItem);
            RemoveKitItemCommand = new AsyncRelayCommand(RemoveKitItemAsync, CanEditOrRemoveKitItem);
            CheckAvailabilityCommand = new AsyncRelayCommand(CheckAvailabilityAsync, CanEditOrDelete);
            RefreshCommand = new AsyncRelayCommand(LoadKitsAsync);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            OpenKitDetailsCommand = new RelayCommand(OpenKitDetails, CanEditOrDelete);
            CopySelectedKitCommand = new RelayCommand(CopySelectedKit, CanEditOrDelete);
            PrintKitListCommand = new RelayCommand(PrintKitList);
            PrintSelectedKitCommand = new RelayCommand(PrintSelectedKit, CanEditOrDelete);
        }

        private async Task LoadKitsAsync()
        {
            try
            {
                var kits = await _kitService.GetAllKitsAsync();
                var previouslySelectedKitId = SelectedKit?.KitID;
                Kits.Clear();
                foreach (var kit in kits)
                {
                    Kits.Add(kit);
                }
                ApplyFilter(previouslySelectedKitId);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error loading kits", ex.Message);
            }
        }

        private async Task LoadKitItemsAsync(int kitID)
        {
            try
            {
                var selectedKitItemId = SelectedKitItem?.KitItemID;
                var items = await _kitService.GetKitItemsAsync(kitID);
                KitItems.Clear();
                foreach (var item in items)
                {
                    KitItems.Add(item);
                }
                SelectedKitItem = KitItems.FirstOrDefault(i => i.KitItemID == selectedKitItemId) ?? KitItems.FirstOrDefault();
                RefreshKitItemSummaries();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error loading kit items", ex.Message);
            }
        }

        private async Task AddKitAsync()
        {
            var newKit = new Kit
            {
                KitNumber = $"KIT-{DateTime.Now:yyyyMMddHHmmss}",
                Name = "New Kit",
                IsActive = true
            };

            var result = await _dialogService.ShowKitEditDialogAsync(newKit, isNew: true);
            if (result)
            {
                try
                {
                    var id = await _kitService.CreateKitAsync(newKit);
                    newKit.KitID = id;
                    Kits.Insert(0, newKit);
                    ApplyFilter(newKit.KitID);
                    await _dialogService.ShowInfoAsync("Success", "Kit created successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error creating kit", ex.Message);
                }
            }
        }

        private async Task EditKitAsync()
        {
            if (SelectedKit == null) return;

            var clone = new Kit
            {
                KitID = SelectedKit.KitID,
                KitNumber = SelectedKit.KitNumber,
                Name = SelectedKit.Name,
                Description = SelectedKit.Description,
                Category = SelectedKit.Category,
                IsActive = SelectedKit.IsActive,
                CreatedByUserID = SelectedKit.CreatedByUserID,
                CreatedAt = SelectedKit.CreatedAt,
                UpdatedAt = SelectedKit.UpdatedAt
            };

            var result = await _dialogService.ShowKitEditDialogAsync(clone, isNew: false);
            if (result)
            {
                try
                {
                    await _kitService.UpdateKitAsync(clone);
                    var index = Kits.IndexOf(SelectedKit);
                    if (index >= 0) Kits[index] = clone;
                    ApplyFilter(clone.KitID);
                    await _dialogService.ShowInfoAsync("Success", "Kit updated successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error updating kit", ex.Message);
                }
            }
        }

        private async Task DeleteKitAsync()
        {
            if (SelectedKit == null) return;

            var kitName = SelectedKit.Name;
            var confirmed = await _dialogService.ShowConfirmAsync(
                "Delete Kit",
                $"Delete kit '{kitName}' and remove every item line attached to it?");

            if (confirmed)
            {
                try
                {
                    await _kitService.DeleteKitAsync(SelectedKit.KitID);
                    Kits.Remove(SelectedKit);
                    SelectedKit = null;
                    ApplyFilter();
                    await _dialogService.ShowInfoAsync("Success", "Kit deleted successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error deleting kit", ex.Message);
                }
            }
        }

        private async Task ViewKitItemsAsync()
        {
            if (SelectedKit == null) return;
            await LoadKitItemsAsync(SelectedKit.KitID);
        }

        private async Task AddKitItemAsync()
        {
            if (SelectedKit == null) return;

            var newKitItem = new KitItem
            {
                KitID = SelectedKit.KitID,
                Quantity = 1,
                IsOptional = false
            };

            var result = await _dialogService.ShowKitItemEditDialogAsync(newKitItem, isNew: true);
            if (result)
            {
                try
                {
                    var id = await _kitService.AddKitItemAsync(newKitItem);
                    newKitItem.KitItemID = id;
                    KitItems.Add(newKitItem);
                    SelectedKitItem = newKitItem;
                    RefreshKitItemSummaries();
                    await _dialogService.ShowInfoAsync("Success", "Item added to kit successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error adding item to kit", ex.Message);
                }
            }
        }

        private async Task EditKitItemAsync()
        {
            if (SelectedKitItem == null) return;

            var clone = new KitItem
            {
                KitItemID = SelectedKitItem.KitItemID,
                KitID = SelectedKitItem.KitID,
                ItemID = SelectedKitItem.ItemID,
                ItemNumber = SelectedKitItem.ItemNumber,
                ItemName = SelectedKitItem.ItemName,
                Quantity = SelectedKitItem.Quantity,
                IsOptional = SelectedKitItem.IsOptional
            };

            var result = await _dialogService.ShowKitItemEditDialogAsync(clone, isNew: false);
            if (result)
            {
                try
                {
                    await _kitService.UpdateKitItemAsync(clone);
                    var index = KitItems.IndexOf(SelectedKitItem);
                    if (index >= 0) KitItems[index] = clone;
                    SelectedKitItem = clone;
                    RefreshKitItemSummaries();
                    await _dialogService.ShowInfoAsync("Success", "Kit item updated successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error updating kit item", ex.Message);
                }
            }
        }

        private async Task RemoveKitItemAsync()
        {
            if (SelectedKitItem == null) return;

            var itemName = ValueOrNotRecorded(SelectedKitItem.ItemName);
            var confirmed = await _dialogService.ShowConfirmAsync(
                "Remove Item from Kit",
                $"Remove '{itemName}' from this kit?");

            if (confirmed)
            {
                try
                {
                    await _kitService.RemoveKitItemAsync(SelectedKitItem.KitItemID);
                    KitItems.Remove(SelectedKitItem);
                    SelectedKitItem = KitItems.FirstOrDefault();
                    RefreshKitItemSummaries();
                    await _dialogService.ShowInfoAsync("Success", "Item removed from kit successfully");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowErrorAsync("Error removing item from kit", ex.Message);
                }
            }
        }

        private async Task CheckAvailabilityAsync()
        {
            if (SelectedKit == null) return;

            try
            {
                var isAvailable = await _kitService.CheckKitAvailabilityAsync(SelectedKit.KitID);
                var message = isAvailable
                    ? $"{SelectedKit.Name} is ready. All required kit items have enough available quantity."
                    : $"{SelectedKit.Name} is short. Review the required item lines before promising or staging this kit.";
                await _dialogService.ShowInfoAsync("Kit Availability", message);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error checking kit availability", ex.Message);
            }
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            SelectedFilter = "Active";
            ApplyFilter();
        }

        private void OpenKitDetails()
        {
            if (SelectedKit == null) return;

            var details = BuildSelectedKitDetails();
            _dialogService.ShowInfo(details, $"Kit Details - {ValueOrNotRecorded(SelectedKit.Name)}");
        }

        private void CopySelectedKit()
        {
            if (SelectedKit == null) return;

            try
            {
                System.Windows.Clipboard.SetText(BuildSelectedKitDetails());
                _dialogService.ShowInfo("Kit details copied to the clipboard.", "Copy Kit Details");
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to copy kit details: {ex.Message}", "Copy Failed");
            }
        }

        private void PrintKitList()
        {
            if (FilteredKits.Count == 0)
            {
                _dialogService.ShowInfo("There are no kits to print for the current filter.", "Kit Directory");
                return;
            }

            try
            {
                var doc = CreateKitDocument("Kit Directory", fontSize: 11);
                doc.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:yyyy-MM-dd HH:mm} | {KitResultsSummary}"))
                {
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                var table = new Table { CellSpacing = 0 };
                table.Columns.Add(new TableColumn { Width = new GridLength(120) });
                table.Columns.Add(new TableColumn { Width = new GridLength(210) });
                table.Columns.Add(new TableColumn { Width = new GridLength(150) });
                table.Columns.Add(new TableColumn { Width = new GridLength(90) });
                table.Columns.Add(new TableColumn { Width = new GridLength(230) });

                var group = new TableRowGroup();
                table.RowGroups.Add(group);
                AddPrintRow(group, true, "Kit #", "Name", "Category", "Status", "Description");
                foreach (var kit in FilteredKits)
                {
                    AddPrintRow(group, false, kit.KitNumber, kit.Name, kit.Category, kit.IsActive ? "Active" : "Inactive", kit.Description);
                }

                doc.Blocks.Add(table);
                _dialogService.ShowPrintPreview(doc, "Kit Directory", string.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print kit directory: {ex.Message}", "Print Failed");
            }
        }

        private void PrintSelectedKit()
        {
            if (SelectedKit == null) return;

            try
            {
                var kit = SelectedKit;
                var doc = CreateKitDocument($"Kit Pick Sheet - {ValueOrNotRecorded(kit.Name)}");
                var detailTable = CreateKeyValueTable();
                var detailGroup = detailTable.RowGroups[0];
                AddKeyValueRow(detailGroup, "Kit #:", kit.KitNumber);
                AddKeyValueRow(detailGroup, "Name:", kit.Name);
                AddKeyValueRow(detailGroup, "Category:", kit.Category);
                AddKeyValueRow(detailGroup, "Status:", kit.IsActive ? "Active" : "Inactive");
                AddKeyValueRow(detailGroup, "Description:", kit.Description);
                AddKeyValueRow(detailGroup, "Advisor note:", "Check availability before staging this kit and confirm required item quantities before checkout.");
                doc.Blocks.Add(detailTable);

                doc.Blocks.Add(new Paragraph(new Bold(new Run("Kit Items")))
                {
                    FontSize = 15,
                    Margin = new Thickness(0, 14, 0, 6)
                });

                var itemTable = new Table { CellSpacing = 0 };
                itemTable.Columns.Add(new TableColumn { Width = new GridLength(130) });
                itemTable.Columns.Add(new TableColumn { Width = new GridLength(280) });
                itemTable.Columns.Add(new TableColumn { Width = new GridLength(80) });
                itemTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
                var itemGroup = new TableRowGroup();
                itemTable.RowGroups.Add(itemGroup);
                AddPrintRow(itemGroup, true, "Item #", "Item", "Qty", "Required");
                foreach (var item in KitItems)
                {
                    AddPrintRow(itemGroup, false, item.ItemNumber, item.ItemName, item.Quantity.ToString(), item.IsOptional ? "Optional" : "Required");
                }
                doc.Blocks.Add(itemTable);

                _dialogService.ShowPrintPreview(doc, $"Kit {kit.KitNumber}", string.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print kit sheet: {ex.Message}", "Print Failed");
            }
        }

        private string BuildSelectedKitDetails()
        {
            if (SelectedKit == null)
                return string.Empty;

            var kit = SelectedKit;
            var details = new StringBuilder();
            details.AppendLine($"Kit #: {ValueOrNotRecorded(kit.KitNumber)}");
            details.AppendLine($"Name: {ValueOrNotRecorded(kit.Name)}");
            details.AppendLine($"Category: {ValueOrNotRecorded(kit.Category)}");
            details.AppendLine($"Status: {(kit.IsActive ? "Active" : "Inactive")}");
            details.AppendLine($"Updated: {kit.UpdatedAt:yyyy-MM-dd HH:mm}");
            details.AppendLine();
            details.AppendLine(ValueOrNotRecorded(kit.Description));
            details.AppendLine();
            details.AppendLine("Kit items:");
            if (KitItems.Count == 0)
            {
                details.AppendLine("- No items assigned.");
            }
            else
            {
                foreach (var item in KitItems)
                {
                    details.AppendLine($"- {ValueOrNotRecorded(item.ItemNumber)} | {ValueOrNotRecorded(item.ItemName)} | Qty {item.Quantity} | {(item.IsOptional ? "Optional" : "Required")}");
                }
            }
            details.AppendLine();
            details.AppendLine("Next steps: check availability, stage required items, then use rentals to complete checkout.");
            return details.ToString();
        }

        private void ApplyFilter(int? preferredKitId = null)
        {
            FilteredKits.Clear();

            var filtered = Kits.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim();
                filtered = filtered.Where(k =>
                    Contains(k.KitNumber, search) ||
                    Contains(k.Name, search) ||
                    Contains(k.Description, search) ||
                    Contains(k.Category, search));
            }

            filtered = SelectedFilter switch
            {
                "Active" => filtered.Where(k => k.IsActive),
                "Inactive" => filtered.Where(k => !k.IsActive),
                _ => filtered
            };

            foreach (var kit in filtered)
            {
                FilteredKits.Add(kit);
            }

            var selectedKit = preferredKitId.HasValue
                ? FilteredKits.FirstOrDefault(k => k.KitID == preferredKitId.Value)
                : FilteredKits.FirstOrDefault(k => SelectedKit != null && k.KitID == SelectedKit.KitID);
            SelectedKit = selectedKit ?? FilteredKits.FirstOrDefault();

            OnPropertyChanged(nameof(KitResultsSummary));
            OnPropertyChanged(nameof(SelectedKitSummary));
            OnPropertyChanged(nameof(SelectedKitDetail));
            OnPropertyChanged(nameof(SelectedKitAvailabilitySummary));
            PrintKitListCommand.NotifyCanExecuteChanged();
        }

        private void RefreshKitItemSummaries()
        {
            OnPropertyChanged(nameof(KitItemsSummary));
            OnPropertyChanged(nameof(SelectedKitSummary));
            OnPropertyChanged(nameof(SelectedKitDetail));
            OnPropertyChanged(nameof(SelectedKitItemSummary));
        }

        private bool CanEditOrDelete() => SelectedKit != null;

        private bool CanEditOrRemoveKitItem() => SelectedKitItem != null;

        static bool Contains(string? source, string search) =>
            !string.IsNullOrWhiteSpace(source) && source.Contains(search, StringComparison.OrdinalIgnoreCase);

        static FlowDocument CreateKitDocument(string title, double fontSize = 16)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(36),
                FontFamily = new System.Windows.Media.FontFamily("Calibri"),
                FontSize = fontSize
            };

            doc.Blocks.Add(new Paragraph(new Bold(new Run(title)))
            {
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            return doc;
        }

        static Table CreateKeyValueTable()
        {
            var table = new Table();
            table.Columns.Add(new TableColumn { Width = new GridLength(150) });
            table.Columns.Add(new TableColumn());
            table.RowGroups.Add(new TableRowGroup());
            return table;
        }

        static void AddKeyValueRow(TableRowGroup group, string label, string? value)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { FontWeight = FontWeights.Bold }));
            row.Cells.Add(new TableCell(new Paragraph(new Run(ValueOrNotRecorded(value)))));
            group.Rows.Add(row);
        }

        static void AddPrintRow(TableRowGroup group, bool isHeader, params string?[] values)
        {
            var row = new TableRow();
            foreach (var value in values)
            {
                var paragraph = new Paragraph(new Run(ValueOrNotRecorded(value)))
                {
                    Margin = new Thickness(3),
                    FontSize = isHeader ? 10 : 9,
                    FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal
                };
                var cell = new TableCell(paragraph)
                {
                    BorderBrush = System.Windows.Media.Brushes.Gray,
                    BorderThickness = new Thickness(0.5),
                    Padding = new Thickness(2)
                };
                row.Cells.Add(cell);
            }
            group.Rows.Add(row);
        }

        static string ValueOrNotRecorded(string? value) => string.IsNullOrWhiteSpace(value) ? "Not recorded" : value;
    }
}
