using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
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
        private const int MaxDirectoryPrintRows = 250;
        private const int MaxSelectedKitHandoffRows = 100;
        private const int MaxSelectedKitPrintRows = 250;
        private const int MaxVisibleFilteredKitRows = 500;
        private readonly KitService _kitService;
        private readonly IDialogService _dialogService;
        private bool _isLoadingKits;
        private bool _isLoadingKitItems;
        private int _kitItemLoadVersion;
        private int _matchedKitCount;
        private int _omittedFilteredKitCount;

        public ObservableCollection<Kit> Kits { get; }
        public ObservableCollection<Kit> FilteredKits { get; }
        public ObservableCollection<KitItem> KitItems { get; }

        public bool IsLoadingKits
        {
            get => _isLoadingKits;
            private set
            {
                if (SetProperty(ref _isLoadingKits, value))
                {
                    RaiseDirectoryStateChanged();
                    RaiseCommandStates();
                }
            }
        }

        public bool IsLoadingKitItems
        {
            get => _isLoadingKitItems;
            private set
            {
                if (SetProperty(ref _isLoadingKitItems, value))
                {
                    RaiseKitItemStateChanged();
                    RaiseCommandStates();
                }
            }
        }

        public bool IsKitInteractionBusy => IsLoadingKits;

        public bool IsKitItemInteractionBusy => IsLoadingKits || IsLoadingKitItems;

        public bool IsKitDirectoryEmptyVisible => !IsKitInteractionBusy && FilteredKits.Count == 0;

        public bool IsKitItemsEmptyVisible => !IsKitItemInteractionBusy && SelectedKit != null && KitItems.Count == 0;

        public bool IsKitDirectoryPrintAvailable => !IsKitInteractionBusy && FilteredKits.Count > 0;

        public int FullFilteredKitCount => _matchedKitCount;

        public int FilteredKitOmittedCount => _omittedFilteredKitCount;

        public bool IsKitFilterWindowCapped => FilteredKitOmittedCount > 0;

        public string KitVisibleWindowSummary
        {
            get
            {
                if (IsLoadingKits) return "Kit rows are loading; the visible grid window will refresh shortly.";
                if (FilteredKits.Count == 0) return "No kit rows match the current search and status filter.";
                if (!IsKitFilterWindowCapped) return "All matching kit rows are visible in the grid.";

                return $"Showing first {FilteredKits.Count} of {FullFilteredKitCount} matching kit rows; {FilteredKitOmittedCount} held out of the grid for responsiveness.";
            }
        }

        public string KitResultsSummary
        {
            get
            {
                if (IsLoadingKits) return "Loading kit directory...";
                var active = Kits.Count(k => k.IsActive);
                var inactive = Kits.Count - active;
                var matched = FullFilteredKitCount;
                var window = IsKitFilterWindowCapped ? $" first {FilteredKits.Count} shown" : $" {FilteredKits.Count} shown";
                return $"{matched} matching kit{(matched == 1 ? string.Empty : "s")} |{window} | {active} active | {inactive} inactive";
            }
        }

        public string KitFilterSummary
        {
            get
            {
                if (IsLoadingKits) return "Filter and search are paused while kit rows load.";

                var status = SelectedFilter == "All" ? "all kits" : SelectedFilter.ToLowerInvariant() + " kits";
                var prefix = string.IsNullOrWhiteSpace(SearchText)
                    ? $"Showing {status}."
                    : $"Showing {status} matching \"{SearchText.Trim()}\".";

                return IsKitFilterWindowCapped
                    ? $"{prefix} Showing the first {FilteredKits.Count} matches so the grid stays responsive."
                    : prefix;
            }
        }

        public string KitPrintSummary
        {
            get
            {
                if (IsLoadingKits) return "Print is paused while kit rows are loading.";
                if (FilteredKits.Count == 0) return "Print is available after kits are loaded or the filter has matches.";

                var printableRows = Math.Min(FilteredKits.Count, MaxDirectoryPrintRows);
                var omittedFromPrint = Math.Max(0, FullFilteredKitCount - printableRows);
                if (omittedFromPrint > 0)
                {
                    return $"Ready to print the first {printableRows} of {FullFilteredKitCount} matching kit rows; {omittedFromPrint} omitted for preview speed.";
                }

                return $"Ready to print {FullFilteredKitCount} visible kit row{(FullFilteredKitCount == 1 ? string.Empty : "s")}.";
            }
        }

        public string SelectedKitHandoffSummary
        {
            get
            {
                if (SelectedKit == null) return "Select a kit to copy handoff details with membership context.";
                if (IsLoadingKitItems) return "Copy and detail handoffs wait until kit membership finishes loading.";
                if (KitItems.Count == 0) return "Handoff details include the selected kit and note that no item lines are assigned.";

                var shown = Math.Min(KitItems.Count, MaxSelectedKitHandoffRows);
                var omitted = KitItems.Count - shown;
                return omitted > 0
                    ? $"Copy/details include the first {shown} of {KitItems.Count} item lines; {omitted} older lines are summarized for responsiveness."
                    : $"Copy/details include all {KitItems.Count} item line{(KitItems.Count == 1 ? string.Empty : "s")}.";
            }
        }

        public string SelectedKitPrintSummary
        {
            get
            {
                if (SelectedKit == null) return "Select a kit to print a staged pick sheet.";
                if (IsLoadingKitItems) return "Kit sheet printing is paused while membership rows load.";
                if (KitItems.Count == 0) return "Kit sheet can print the selected kit details and a no-items notice.";

                var printed = Math.Min(KitItems.Count, MaxSelectedKitPrintRows);
                var omitted = KitItems.Count - printed;
                return omitted > 0
                    ? $"Kit sheet prints the first {printed} of {KitItems.Count} item lines; {omitted} omitted for preview speed."
                    : $"Kit sheet prints all {KitItems.Count} item line{(KitItems.Count == 1 ? string.Empty : "s")}.";
            }
        }

        public string KitEmptyStateTitle
        {
            get
            {
                if (Kits.Count == 0) return "No kits saved yet";
                return string.IsNullOrWhiteSpace(SearchText) && SelectedFilter == "All"
                    ? "No kits to show"
                    : "No kits match this filter";
            }
        }

        public string KitEmptyStateMessage
        {
            get
            {
                if (Kits.Count == 0)
                {
                    return "Add the first reusable kit so staff can stage grouped items, confirm availability, and print a pick sheet from one workflow.";
                }

                return "Clear the search, change the status filter, or add a kit that matches the current shop language.";
            }
        }

        public string KitItemLoadSummary
        {
            get
            {
                if (SelectedKit == null) return "Select a kit to load required and optional item lines.";
                if (IsLoadingKitItems) return $"Loading membership for {ValueOrNotRecorded(SelectedKit.KitNumber)}...";
                if (KitItems.Count == 0) return "No item lines are assigned to this kit yet.";
                return $"{KitItems.Count} item line{(KitItems.Count == 1 ? string.Empty : "s")} ready for availability review and pick-sheet printing.";
            }
        }

        public string KitItemsEmptyStateTitle => SelectedKit == null
            ? "No kit selected"
            : "No items assigned to this kit";

        public string KitItemsEmptyStateMessage => SelectedKit == null
            ? "Choose a kit from the directory to load its membership lines."
            : "Add required and optional item lines so availability checks and printed handoffs have enough detail.";

        public string SelectedKitSummary => SelectedKit == null
            ? "Select a kit to review membership, check availability, copy details, print a pick sheet, or maintain its item list."
            : $"{ValueOrNotRecorded(SelectedKit.KitNumber)} | {ValueOrNotRecorded(SelectedKit.Name)} | {(IsLoadingKitItems ? "loading" : KitItems.Count.ToString())} item line{(KitItems.Count == 1 ? string.Empty : "s")} | {(SelectedKit.IsActive ? "Active" : "Inactive")}";

        public string SelectedKitDetail => SelectedKit == null
            ? "No kit selected. Choose a row from the directory to see the operational detail here."
            : $"Kit # {ValueOrNotRecorded(SelectedKit.KitNumber)}\nName: {ValueOrNotRecorded(SelectedKit.Name)}\nCategory: {ValueOrNotRecorded(SelectedKit.Category)}\nStatus: {(SelectedKit.IsActive ? "Active" : "Inactive")}\nItems: {(IsLoadingKitItems ? "Loading" : KitItems.Count.ToString())}\nUpdated: {SelectedKit.UpdatedAt:yyyy-MM-dd HH:mm}\n\n{ValueOrNotRecorded(SelectedKit.Description)}";

        public string KitItemsSummary => SelectedKit == null
            ? "No kit selected"
            : IsLoadingKitItems
                ? $"Loading item lines for {ValueOrNotRecorded(SelectedKit.KitNumber)}"
                : $"{KitItems.Count} item line{(KitItems.Count == 1 ? string.Empty : "s")} in {ValueOrNotRecorded(SelectedKit.KitNumber)} | {KitItems.Count(i => !i.IsOptional)} required | {KitItems.Count(i => i.IsOptional)} optional";

        public string SelectedKitItemSummary => SelectedKitItem == null
            ? "Select a kit item to edit quantity, mark optional, or remove it from this kit."
            : $"{ValueOrNotRecorded(SelectedKitItem.ItemNumber)} | {ValueOrNotRecorded(SelectedKitItem.ItemName)} | Qty {SelectedKitItem.Quantity} | {(SelectedKitItem.IsOptional ? "Optional" : "Required")}";

        public string SelectedKitAvailabilitySummary => SelectedKit == null
            ? "Availability check is ready once a kit is selected."
            : IsLoadingKitItems
                ? "Membership is loading; availability checks are paused until item lines are ready."
                : "Use Check Availability before promising the kit; required item quantities are checked against current stock.";

        private Kit? _selectedKit;
        public Kit? SelectedKit
        {
            get => _selectedKit;
            set
            {
                if (SetProperty(ref _selectedKit, value))
                {
                    RaiseCommandStates();
                    OnPropertyChanged(nameof(SelectedKitSummary));
                    OnPropertyChanged(nameof(SelectedKitDetail));
                    OnPropertyChanged(nameof(SelectedKitHandoffSummary));
                    OnPropertyChanged(nameof(SelectedKitPrintSummary));
                    OnPropertyChanged(nameof(KitItemsSummary));
                    OnPropertyChanged(nameof(SelectedKitAvailabilitySummary));
                    OnPropertyChanged(nameof(KitItemLoadSummary));
                    OnPropertyChanged(nameof(KitItemsEmptyStateTitle));
                    OnPropertyChanged(nameof(KitItemsEmptyStateMessage));

                    if (value != null)
                    {
                        _ = LoadKitItemsAsync(value.KitID);
                    }
                    else
                    {
                        _kitItemLoadVersion++;
                        ClearKitItemsForReload();
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

            LoadKitsCommand = new AsyncRelayCommand(LoadKitsAsync, () => !IsKitInteractionBusy);
            AddKitCommand = new AsyncRelayCommand(AddKitAsync, () => !IsKitInteractionBusy);
            EditKitCommand = new AsyncRelayCommand(EditKitAsync, CanEditOrDelete);
            DeleteKitCommand = new AsyncRelayCommand(DeleteKitAsync, CanEditOrDelete);
            ViewKitItemsCommand = new AsyncRelayCommand(ViewKitItemsAsync, CanEditOrDelete);
            AddKitItemCommand = new AsyncRelayCommand(AddKitItemAsync, CanMaintainKitItems);
            EditKitItemCommand = new AsyncRelayCommand(EditKitItemAsync, CanEditOrRemoveKitItem);
            RemoveKitItemCommand = new AsyncRelayCommand(RemoveKitItemAsync, CanEditOrRemoveKitItem);
            CheckAvailabilityCommand = new AsyncRelayCommand(CheckAvailabilityAsync, CanEditOrDelete);
            RefreshCommand = new AsyncRelayCommand(LoadKitsAsync, () => !IsKitInteractionBusy);
            ClearSearchCommand = new RelayCommand(ClearSearch, () => !IsKitInteractionBusy && (!string.IsNullOrWhiteSpace(SearchText) || SelectedFilter != "Active"));
            OpenKitDetailsCommand = new RelayCommand(OpenKitDetails, CanEditOrDelete);
            CopySelectedKitCommand = new RelayCommand(CopySelectedKit, CanEditOrDelete);
            PrintKitListCommand = new RelayCommand(PrintKitList, () => IsKitDirectoryPrintAvailable);
            PrintSelectedKitCommand = new RelayCommand(PrintSelectedKit, CanEditOrDelete);
        }

        private async Task LoadKitsAsync()
        {
            if (IsKitInteractionBusy)
                return;

            IsLoadingKits = true;

            try
            {
                var kits = await _kitService.GetAllKitsAsync();
                var previouslySelectedKitId = SelectedKit?.KitID;
                Kits.Clear();
                foreach (var kit in kits.OrderByDescending(k => k.IsActive).ThenBy(k => k.Name).ThenBy(k => k.KitNumber))
                {
                    Kits.Add(kit);
                }
                ApplyFilter(previouslySelectedKitId);
            }
            catch (Exception ex)
            {
                ClearKitStateAfterLoadFailure();
                await _dialogService.ShowErrorAsync("Error loading kits", $"{ex.Message} Kit rows were cleared until reload succeeds.");
            }
            finally
            {
                IsLoadingKits = false;
            }
        }

        private void ClearKitStateAfterLoadFailure()
        {
            Kits.Clear();
            _matchedKitCount = 0;
            _omittedFilteredKitCount = 0;
            FilteredKits.Clear();
            SelectedKit = null;
            SelectedKitItem = null;
            KitItems.Clear();
            RefreshKitItemSummaries();
            RaiseDirectoryStateChanged();
            OnPropertyChanged(nameof(SelectedKitAvailabilitySummary));
            PrintKitListCommand.NotifyCanExecuteChanged();
        }

        private async Task LoadKitItemsAsync(int kitID)
        {
            var loadVersion = ++_kitItemLoadVersion;
            var selectedKitItemId = SelectedKitItem?.KitItemID;
            ClearKitItemsForReload();
            IsLoadingKitItems = true;

            try
            {
                var items = await _kitService.GetKitItemsAsync(kitID);
                if (loadVersion != _kitItemLoadVersion || SelectedKit?.KitID != kitID)
                    return;

                foreach (var item in items.OrderBy(i => i.IsOptional).ThenBy(i => i.ItemName).ThenBy(i => i.ItemNumber))
                {
                    KitItems.Add(item);
                }
                SelectedKitItem = KitItems.FirstOrDefault(i => i.KitItemID == selectedKitItemId) ?? KitItems.FirstOrDefault();
                RefreshKitItemSummaries();
            }
            catch (Exception ex)
            {
                if (loadVersion != _kitItemLoadVersion)
                    return;

                ClearKitItemsForReload();
                await _dialogService.ShowErrorAsync("Error loading kit items", $"{ex.Message} Kit item rows were cleared until reload succeeds.");
            }
            finally
            {
                if (loadVersion == _kitItemLoadVersion)
                {
                    IsLoadingKitItems = false;
                }
            }
        }

        private void ClearKitItemsForReload()
        {
            KitItems.Clear();
            SelectedKitItem = null;
            RefreshKitItemSummaries();
            RaiseKitItemStateChanged();
        }

        private async Task RefreshKitItemsAfterMutationFailureAsync(string title, string message)
        {
            if (SelectedKit == null)
            {
                await _dialogService.ShowErrorAsync(title, message);
                return;
            }

            try
            {
                await ReloadKitItemsForRecoveryAsync(SelectedKit.KitID);
                await _dialogService.ShowErrorAsync(title, $"{message} Kit item rows were refreshed in case the membership list changed before the failure.");
            }
            catch (Exception refreshEx)
            {
                ClearKitItemsForReload();
                await _dialogService.ShowErrorAsync(title, $"{message} Kit item rows were cleared because refresh also failed: {refreshEx.Message}");
            }
        }

        private async Task ReloadKitItemsForRecoveryAsync(int kitID)
        {
            var selectedKitItemId = SelectedKitItem?.KitItemID;
            ClearKitItemsForReload();
            var items = await _kitService.GetKitItemsAsync(kitID);
            foreach (var item in items.OrderBy(i => i.IsOptional).ThenBy(i => i.ItemName).ThenBy(i => i.ItemNumber))
            {
                KitItems.Add(item);
            }
            SelectedKitItem = KitItems.FirstOrDefault(i => i.KitItemID == selectedKitItemId) ?? KitItems.FirstOrDefault();
            RefreshKitItemSummaries();
        }

        private async Task AddKitAsync()
        {
            if (IsKitInteractionBusy) return;

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
            if (SelectedKit == null || IsKitInteractionBusy) return;

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
            if (SelectedKit == null || IsKitInteractionBusy) return;

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
            if (SelectedKit == null || IsKitInteractionBusy) return;
            await LoadKitItemsAsync(SelectedKit.KitID);
        }

        private async Task AddKitItemAsync()
        {
            if (SelectedKit == null || IsKitItemInteractionBusy) return;

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
                    await RefreshKitItemsAfterMutationFailureAsync("Error adding item to kit", ex.Message);
                }
            }
        }

        private async Task EditKitItemAsync()
        {
            if (SelectedKitItem == null || IsKitItemInteractionBusy) return;

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
                    await RefreshKitItemsAfterMutationFailureAsync("Error updating kit item", ex.Message);
                }
            }
        }

        private async Task RemoveKitItemAsync()
        {
            if (SelectedKitItem == null || IsKitItemInteractionBusy) return;

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
                    await RefreshKitItemsAfterMutationFailureAsync("Error removing item from kit", ex.Message);
                }
            }
        }

        private async Task CheckAvailabilityAsync()
        {
            if (SelectedKit == null || IsKitItemInteractionBusy) return;

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
            if (IsKitInteractionBusy) return;

            SearchText = string.Empty;
            SelectedFilter = "Active";
            ApplyFilter();
        }

        private void OpenKitDetails()
        {
            if (SelectedKit == null || IsKitInteractionBusy) return;

            var details = BuildSelectedKitDetails();
            _dialogService.ShowInfo(details, $"Kit Details - {ValueOrNotRecorded(SelectedKit.Name)}");
        }

        private void CopySelectedKit()
        {
            if (SelectedKit == null || IsKitInteractionBusy) return;

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
            if (IsKitInteractionBusy)
            {
                _dialogService.ShowInfo("Kit directory printing is paused while rows are loading.", "Kit Directory");
                return;
            }

            if (FilteredKits.Count == 0)
            {
                _dialogService.ShowInfo("There are no kits to print for the current filter.", "Kit Directory");
                return;
            }

            try
            {
                var visibleKits = FilteredKits.ToList();
                var printedKits = visibleKits.Take(MaxDirectoryPrintRows).ToList();
                var omittedCount = Math.Max(0, FullFilteredKitCount - printedKits.Count);
                var filterContext = string.IsNullOrWhiteSpace(SearchText)
                    ? $"Status filter: {SelectedFilter}"
                    : $"Status filter: {SelectedFilter} | Search: {SearchText.Trim()}";

                var doc = CreateKitDocument("Kit Directory", fontSize: 11);
                doc.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:yyyy-MM-dd HH:mm} | Matched {FullFilteredKitCount} | Grid window {visibleKits.Count} | Printed {printedKits.Count} | Omitted {omittedCount} | {filterContext}"))
                {
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 8)
                });
                doc.Blocks.Add(new Paragraph(new Run("Review active status, category, and descriptions before staging grouped item sets. Large filtered directories print the first 250 matching rows to keep preview responsive."))
                {
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                var table = new Table { CellSpacing = 0 };
                table.Columns.Add(new TableColumn { Width = new GridLength(1.15, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.85, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.25, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(0.8, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(2.25, GridUnitType.Star) });

                var group = new TableRowGroup();
                table.RowGroups.Add(group);
                AddPrintRow(group, true, "Kit #", "Name", "Category", "Status", "Description");
                foreach (var kit in printedKits)
                {
                    AddPrintRow(group, false, kit.KitNumber, kit.Name, kit.Category, kit.IsActive ? "Active" : "Inactive", kit.Description);
                }

                doc.Blocks.Add(table);
                _dialogService.ShowPrintPreview(doc, "Kit Directory", KitPrintSummary);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print kit directory: {ex.Message}", "Print Failed");
            }
        }

        private void PrintSelectedKit()
        {
            if (SelectedKit == null || IsKitItemInteractionBusy) return;

            try
            {
                var kit = SelectedKit;
                var visibleItems = KitItems.ToList();
                var printedItems = visibleItems.Take(MaxSelectedKitPrintRows).ToList();
                var omittedCount = visibleItems.Count - printedItems.Count;
                var doc = CreateKitDocument($"Kit Pick Sheet - {ValueOrNotRecorded(kit.Name)}");
                doc.Blocks.Add(new Paragraph(new Run($"Prepared {DateTime.Now:yyyy-MM-dd HH:mm} | Item lines {visibleItems.Count} | Printed {printedItems.Count} | Omitted {omittedCount}"))
                {
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 8)
                });
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

                if (omittedCount > 0)
                {
                    doc.Blocks.Add(new Paragraph(new Run($"Large kit membership: printing the first {printedItems.Count} item lines and omitting {omittedCount} for preview speed. Review the live kit membership grid before final staging."))
                    {
                        FontSize = 10,
                        Foreground = Brushes.DarkSlateGray,
                        Margin = new Thickness(0, 0, 0, 8)
                    });
                }
                else if (printedItems.Count == 0)
                {
                    doc.Blocks.Add(new Paragraph(new Run("No item lines are assigned to this kit yet."))
                    {
                        FontSize = 10,
                        Margin = new Thickness(0, 0, 0, 8)
                    });
                }

                var itemTable = new Table { CellSpacing = 0 };
                itemTable.Columns.Add(new TableColumn { Width = new GridLength(1.2, GridUnitType.Star) });
                itemTable.Columns.Add(new TableColumn { Width = new GridLength(2.4, GridUnitType.Star) });
                itemTable.Columns.Add(new TableColumn { Width = new GridLength(0.7, GridUnitType.Star) });
                itemTable.Columns.Add(new TableColumn { Width = new GridLength(0.9, GridUnitType.Star) });
                var itemGroup = new TableRowGroup();
                itemTable.RowGroups.Add(itemGroup);
                AddPrintRow(itemGroup, true, "Item #", "Item", "Qty", "Required");
                foreach (var item in printedItems)
                {
                    AddPrintRow(itemGroup, false, item.ItemNumber, item.ItemName, item.Quantity.ToString(), item.IsOptional ? "Optional" : "Required");
                }
                doc.Blocks.Add(itemTable);

                _dialogService.ShowPrintPreview(doc, $"Kit {kit.KitNumber}", SelectedKitPrintSummary);
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
            if (IsLoadingKitItems)
            {
                details.AppendLine("- Kit items are still loading.");
            }
            else if (KitItems.Count == 0)
            {
                details.AppendLine("- No items assigned.");
            }
            else
            {
                var handoffItems = KitItems.Take(MaxSelectedKitHandoffRows).ToList();
                foreach (var item in handoffItems)
                {
                    details.AppendLine($"- {ValueOrNotRecorded(item.ItemNumber)} | {ValueOrNotRecorded(item.ItemName)} | Qty {item.Quantity} | {(item.IsOptional ? "Optional" : "Required")}");
                }

                var omittedCount = KitItems.Count - handoffItems.Count;
                if (omittedCount > 0)
                {
                    details.AppendLine($"- {omittedCount} additional item line{(omittedCount == 1 ? string.Empty : "s")} omitted from this handoff summary for responsiveness.");
                }
            }
            details.AppendLine();
            details.AppendLine(SelectedKitHandoffSummary);
            details.AppendLine("Next steps: check availability, stage required items, then use rentals to complete checkout.");
            return details.ToString();
        }

        private void ApplyFilter(int? preferredKitId = null)
        {
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

            var matched = filtered
                .OrderByDescending(k => k.IsActive)
                .ThenBy(k => k.Name)
                .ThenBy(k => k.KitNumber)
                .ToList();
            _matchedKitCount = matched.Count;
            var visible = matched.Take(MaxVisibleFilteredKitRows).ToList();
            _omittedFilteredKitCount = Math.Max(0, matched.Count - visible.Count);
            ReplaceFilteredKits(visible);

            var selectedKit = preferredKitId.HasValue
                ? FilteredKits.FirstOrDefault(k => k.KitID == preferredKitId.Value)
                : FilteredKits.FirstOrDefault(k => SelectedKit != null && k.KitID == SelectedKit.KitID);
            SelectedKit = selectedKit ?? FilteredKits.FirstOrDefault();

            RaiseDirectoryStateChanged();
            RaiseCommandStates();
        }

        private void ReplaceFilteredKits(IReadOnlyList<Kit> visibleKits)
        {
            var unchanged = FilteredKits.Count == visibleKits.Count;
            if (unchanged)
            {
                for (var i = 0; i < visibleKits.Count; i++)
                {
                    if (!ReferenceEquals(FilteredKits[i], visibleKits[i]))
                    {
                        unchanged = false;
                        break;
                    }
                }
            }

            if (unchanged) return;

            FilteredKits.Clear();
            foreach (var kit in visibleKits)
            {
                FilteredKits.Add(kit);
            }
        }

        private void RefreshKitItemSummaries()
        {
            OnPropertyChanged(nameof(KitItemsSummary));
            OnPropertyChanged(nameof(SelectedKitSummary));
            OnPropertyChanged(nameof(SelectedKitDetail));
            OnPropertyChanged(nameof(SelectedKitHandoffSummary));
            OnPropertyChanged(nameof(SelectedKitPrintSummary));
            OnPropertyChanged(nameof(SelectedKitItemSummary));
            OnPropertyChanged(nameof(SelectedKitAvailabilitySummary));
            OnPropertyChanged(nameof(KitItemLoadSummary));
            OnPropertyChanged(nameof(IsKitItemsEmptyVisible));
        }

        private void RaiseDirectoryStateChanged()
        {
            OnPropertyChanged(nameof(IsKitInteractionBusy));
            OnPropertyChanged(nameof(IsKitDirectoryEmptyVisible));
            OnPropertyChanged(nameof(IsKitDirectoryPrintAvailable));
            OnPropertyChanged(nameof(FullFilteredKitCount));
            OnPropertyChanged(nameof(FilteredKitOmittedCount));
            OnPropertyChanged(nameof(IsKitFilterWindowCapped));
            OnPropertyChanged(nameof(KitVisibleWindowSummary));
            OnPropertyChanged(nameof(KitResultsSummary));
            OnPropertyChanged(nameof(KitFilterSummary));
            OnPropertyChanged(nameof(KitPrintSummary));
            OnPropertyChanged(nameof(KitEmptyStateTitle));
            OnPropertyChanged(nameof(KitEmptyStateMessage));
        }

        private void RaiseKitItemStateChanged()
        {
            OnPropertyChanged(nameof(IsKitItemInteractionBusy));
            OnPropertyChanged(nameof(IsKitItemsEmptyVisible));
            OnPropertyChanged(nameof(KitItemsSummary));
            OnPropertyChanged(nameof(SelectedKitSummary));
            OnPropertyChanged(nameof(SelectedKitDetail));
            OnPropertyChanged(nameof(SelectedKitHandoffSummary));
            OnPropertyChanged(nameof(SelectedKitPrintSummary));
            OnPropertyChanged(nameof(SelectedKitAvailabilitySummary));
            OnPropertyChanged(nameof(KitItemLoadSummary));
        }

        private void RaiseCommandStates()
        {
            LoadKitsCommand.NotifyCanExecuteChanged();
            AddKitCommand.NotifyCanExecuteChanged();
            EditKitCommand.NotifyCanExecuteChanged();
            DeleteKitCommand.NotifyCanExecuteChanged();
            ViewKitItemsCommand.NotifyCanExecuteChanged();
            AddKitItemCommand.NotifyCanExecuteChanged();
            EditKitItemCommand.NotifyCanExecuteChanged();
            RemoveKitItemCommand.NotifyCanExecuteChanged();
            CheckAvailabilityCommand.NotifyCanExecuteChanged();
            RefreshCommand.NotifyCanExecuteChanged();
            ClearSearchCommand.NotifyCanExecuteChanged();
            OpenKitDetailsCommand.NotifyCanExecuteChanged();
            CopySelectedKitCommand.NotifyCanExecuteChanged();
            PrintKitListCommand.NotifyCanExecuteChanged();
            PrintSelectedKitCommand.NotifyCanExecuteChanged();
        }

        private bool CanEditOrDelete() => SelectedKit != null && !IsKitInteractionBusy && !IsLoadingKitItems;

        private bool CanMaintainKitItems() => SelectedKit != null && !IsKitItemInteractionBusy;

        private bool CanEditOrRemoveKitItem() => SelectedKitItem != null && !IsKitItemInteractionBusy;

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
