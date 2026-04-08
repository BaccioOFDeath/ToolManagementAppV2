using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
                    if (value != null)
                    {
                        _ = LoadKitItemsAsync(value.KitID);
                    }
                    else
                    {
                        KitItems.Clear();
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
        }

        private async Task LoadKitsAsync()
        {
            try
            {
                var kits = await _kitService.GetAllKitsAsync();
                Kits.Clear();
                foreach (var kit in kits)
                {
                    Kits.Add(kit);
                }
                ApplyFilter();
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
                var items = await _kitService.GetKitItemsAsync(kitID);
                KitItems.Clear();
                foreach (var item in items)
                {
                    KitItems.Add(item);
                }
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
                    ApplyFilter();
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
                    ApplyFilter();
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

            var confirmed = await _dialogService.ShowConfirmAsync(
                "Delete Kit",
                $"Are you sure you want to delete kit '{SelectedKit.Name}'? This will also remove all items from the kit.");

            if (confirmed)
            {
                try
                {
                    await _kitService.DeleteKitAsync(SelectedKit.KitID);
                    Kits.Remove(SelectedKit);
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

            var confirmed = await _dialogService.ShowConfirmAsync(
                "Remove Item from Kit",
                $"Are you sure you want to remove this item from the kit?");

            if (confirmed)
            {
                try
                {
                    await _kitService.RemoveKitItemAsync(SelectedKitItem.KitItemID);
                    KitItems.Remove(SelectedKitItem);
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
                    ? "All required items in this kit are available."
                    : "Some required items in this kit are not available in sufficient quantities.";
                await _dialogService.ShowInfoAsync("Kit Availability", message);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync("Error checking kit availability", ex.Message);
            }
        }

        private void ApplyFilter()
        {
            FilteredKits.Clear();

            var filtered = Kits.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLowerInvariant();
                filtered = filtered.Where(k =>
                    k.KitNumber.ToLowerInvariant().Contains(search) ||
                    k.Name.ToLowerInvariant().Contains(search) ||
                    k.Description.ToLowerInvariant().Contains(search) ||
                    k.Category.ToLowerInvariant().Contains(search));
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
        }

        private bool CanEditOrDelete() => SelectedKit != null;

        private bool CanEditOrRemoveKitItem() => SelectedKitItem != null;
    }
}
