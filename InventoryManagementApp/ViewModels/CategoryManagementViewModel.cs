// ViewModels/CategoryManagementViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using InventoryManagementApp.Services.Categories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WpfMessageBox = System.Windows.MessageBox;

namespace InventoryManagementApp.ViewModels
{
    public sealed class CategoryManagementViewModel : INotifyPropertyChanged
    {
        private const int MaxVisibleFilteredCategoryRows = 500;
        private readonly CategoriesService _service;
        private readonly ILogger<CategoryManagementViewModel> _logger;
        private int _selectedInventoryId;
        private CategoryItem? _selectedCategory;
        private string _categoryName = "";
        private string _searchText = "";
        private string _statusMessage = "Ready to manage category setup.";
        private bool _isBusy;
        private bool _schemaInitialized;
        private bool _loadFailureDialogShown;
        private int _matchedCategoryCount;
        private int _omittedFilteredCategoryCount;

        public ObservableCollection<CategoryItem> Categories { get; } = new();
        public ObservableCollection<CategoryItem> FilteredCategories { get; } = new();

        public int SelectedInventoryId
        {
            get => _selectedInventoryId;
            set
            {
                if (_selectedInventoryId == value) return;
                _selectedInventoryId = value;
                OnPropertyChanged();
                RaiseCommandStates();
                if (_schemaInitialized)
                {
                    LoadCategoriesAsync();
                }
            }
        }

        public CategoryItem? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory == value) return;
                _selectedCategory = value;
                OnPropertyChanged();
                CategoryName = value?.Name ?? "";
                RaiseSelectedCategoryProperties();
                RaiseCommandStates();
            }
        }

        public string CategoryName
        {
            get => _categoryName;
            set
            {
                if (_categoryName == value) return;
                _categoryName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CategoryNameStatus));
                OnPropertyChanged(nameof(SelectedCategoryNextAction));
                RaiseCommandStates();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
                RaiseCommandStates();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if (_statusMessage == value) return;
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCategoryInteractionBusy));
                OnPropertyChanged(nameof(IsCategoryActionAvailable));
                OnPropertyChanged(nameof(IsSelectedCategoryActionAvailable));
                RaiseDirectoryProperties();
                RaiseCommandStates();
            }
        }

        public bool IsCategoryInteractionBusy => IsBusy;

        public bool IsCategoryActionAvailable => !IsCategoryInteractionBusy;

        public bool IsSelectedCategoryActionAvailable => !IsCategoryInteractionBusy && SelectedCategory != null;

        public bool IsDirectoryPrintAvailable => !IsCategoryInteractionBusy && FilteredCategories.Count > 0;

        public bool IsCategoryEmptyStateVisible => !IsCategoryInteractionBusy && FilteredCategories.Count == 0;

        public int FullFilteredCategoryCount => _matchedCategoryCount;

        public int FilteredCategoryOmittedCount => _omittedFilteredCategoryCount;

        public bool IsCategoryFilterWindowCapped => FilteredCategoryOmittedCount > 0;

        public string CategoryResultsSummary
        {
            get
            {
                if (IsCategoryInteractionBusy) return "Loading category directory...";

                var visible = FilteredCategories.Count;
                var matched = FullFilteredCategoryCount;
                var noun = matched == 1 ? "category" : "categories";
                var prefix = string.IsNullOrWhiteSpace(SearchText)
                    ? $"{matched} {noun} match the current inventory area"
                    : $"{matched} of {Categories.Count} categor{(matched == 1 ? "y" : "ies")} match \"{SearchText.Trim()}\"";

                return IsCategoryFilterWindowCapped
                    ? $"{visible} of {prefix} shown; {FilteredCategoryOmittedCount} held out of the grid for responsiveness"
                    : $"{visible} {noun} shown";
            }
        }

        public string CategorySetupSummary => Categories.Count == 0
            ? "No categories have been linked to this inventory area yet."
            : $"{Categories.Count} categor{(Categories.Count == 1 ? "y" : "ies")} support filtering, item setup, and advisor search.";

        public string CategoryFilterSummary
        {
            get
            {
                if (IsCategoryInteractionBusy) return "Search and setup actions are paused while category rows load.";
                if (string.IsNullOrWhiteSpace(SearchText)) return "Showing every linked category.";

                var suffix = IsCategoryFilterWindowCapped
                    ? $" Showing the first {FilteredCategories.Count} matches."
                    : string.Empty;
                return $"Filter active: \"{SearchText.Trim()}\".{suffix}";
            }
        }

        public string CategoryVisibleWindowSummary
        {
            get
            {
                if (IsCategoryInteractionBusy) return "Grid rows are loading.";
                if (!IsCategoryFilterWindowCapped) return "All matching categories are visible in the grid.";

                return $"Showing first {FilteredCategories.Count} of {FullFilteredCategoryCount} matching categories; {FilteredCategoryOmittedCount} additional matches are summarized to keep filtering fast.";
            }
        }

        public string CategoryPrintSummary
        {
            get
            {
                if (IsCategoryInteractionBusy) return "Print is paused while category rows are loading.";
                if (FilteredCategories.Count == 0) return "Print is available after categories are loaded or the filter has matches.";

                var printableRows = Math.Min(FilteredCategories.Count, 250);
                var omittedFromPrint = Math.Max(0, FullFilteredCategoryCount - printableRows);
                var filterContext = string.IsNullOrWhiteSpace(SearchText) ? "visible category" : "filtered category";
                return omittedFromPrint > 0
                    ? $"Ready to print the first {printableRows} of {FullFilteredCategoryCount} {filterContext} row{(FullFilteredCategoryCount == 1 ? "" : "s")}; {omittedFromPrint} omitted for preview speed."
                    : $"Ready to print {FullFilteredCategoryCount} {filterContext} row{(FullFilteredCategoryCount == 1 ? "" : "s")}.";
            }
        }

        public string CategoryEmptyStateTitle
        {
            get
            {
                if (Categories.Count == 0) return "No categories linked yet";
                return string.IsNullOrWhiteSpace(SearchText)
                    ? "No categories to show"
                    : "No categories match this filter";
            }
        }

        public string CategoryEmptyStateMessage
        {
            get
            {
                if (Categories.Count == 0)
                {
                    return "Create the first category for this inventory area so item setup, advisor search, and printed directories have a controlled vocabulary.";
                }

                return string.IsNullOrWhiteSpace(SearchText)
                    ? "Refresh the directory or create a category name that matches how staff ask for tools."
                    : "Clear the search, adjust the filter, or create a category name that matches how staff ask for tools.";
            }
        }

        public string CategoryNameStatus
        {
            get
            {
                var name = CategoryName.Trim();
                if (name.Length == 0) return "Enter a category name before creating or saving.";
                var duplicate = Categories.Any(c =>
                    c.CategoryID != SelectedCategory?.CategoryID &&
                    string.Equals(c.Name.Trim(), name, StringComparison.OrdinalIgnoreCase));
                return duplicate
                    ? "Another category already uses this name. Saving may be rejected by the data layer."
                    : "Name is ready for create or save.";
            }
        }

        public string SelectedCategoryTitle => SelectedCategory == null
            ? "No category selected"
            : SelectedCategory.Name;

        public string SelectedCategorySubtitle => SelectedCategory == null
            ? "Select a row to review, rename, print, or copy its setup handoff."
            : $"Category #{SelectedCategory.CategoryID} | {CategoryFilterSummary}";

        public string SelectedCategoryDetail => SelectedCategory == null
            ? "Choose a category to review how advisors and technicians will find matching inventory records."
            : $"Category #{SelectedCategory.CategoryID} is named \"{SelectedCategory.Name}\". Keep names short, familiar, and aligned with how staff ask for items at the counter or shelf.";

        public string SelectedCategoryNextAction
        {
            get
            {
                if (SelectedCategory == null)
                {
                    return Categories.Count == 0
                        ? "Create the first category for this inventory area so item setup can be grouped cleanly."
                        : "Select a category, filter the directory, or create a new category for another workflow group.";
                }

                var proposedName = CategoryName.Trim();
                if (!string.Equals(proposedName, SelectedCategory.Name, StringComparison.Ordinal) && proposedName.Length > 0)
                {
                    return "Unsaved rename detected. Save the name before assigning or reviewing related inventory records.";
                }

                return "Next step: confirm this category matches staff language, then assign matching inventory records from item setup or use search filters to review coverage.";
            }
        }

        public string SelectedCategorySummary => SelectedCategory == null
            ? "Select a category row to open details, rename it, copy the handoff, print the directory, or delete it."
            : $"Selected: #{SelectedCategory.CategoryID} | {SelectedCategory.Name}";

        public string SelectedCategoryChecklist => SelectedCategory == null
            ? "Checklist: create category name, save it, assign matching inventory records, then verify search/filter results."
            : "Checklist: name is clear, matching items are assigned, advisors can find it quickly, and obsolete duplicate categories are removed.";

        public string SelectedCategoryHandoff => SelectedCategory == null
            ? "Admin handoff will appear here after a category is selected."
            : $"Admin handoff: #{SelectedCategory.CategoryID} - {SelectedCategory.Name}. Use for setup review, rename discussion, or printed category directory notes.";

        private readonly AsyncCommand _addCommand;
        private readonly AsyncCommand _saveCommand;
        private readonly AsyncCommand _deleteCommand;
        private readonly AsyncCommand _refreshCommand;
        private readonly AsyncCommand _clearSearchCommand;

        public ICommand AddCommand => _addCommand;
        public ICommand SaveCommand => _saveCommand;
        public ICommand DeleteCommand => _deleteCommand;
        public ICommand RefreshCommand => _refreshCommand;
        public ICommand ClearSearchCommand => _clearSearchCommand;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CategoryManagementViewModel(CategoriesService service, ILogger<CategoryManagementViewModel>? logger = null)
        {
            _service = service;
            _logger = logger ?? NullLogger<CategoryManagementViewModel>.Instance;
            _addCommand = new AsyncCommand(AddAsync, () => !IsCategoryInteractionBusy && !string.IsNullOrWhiteSpace(CategoryName) && SelectedInventoryId > 0);
            _saveCommand = new AsyncCommand(SaveAsync, () => !IsCategoryInteractionBusy && SelectedCategory != null && !string.IsNullOrWhiteSpace(CategoryName));
            _deleteCommand = new AsyncCommand(DeleteAsync, () => !IsCategoryInteractionBusy && SelectedCategory != null);
            _refreshCommand = new AsyncCommand(LoadAsync, () => !IsCategoryInteractionBusy && SelectedInventoryId > 0);
            _clearSearchCommand = new AsyncCommand(ClearSearchAsync, () => !IsCategoryInteractionBusy && !string.IsNullOrWhiteSpace(SearchText));
        }

        private async void LoadCategoriesAsync()
        {
            try
            {
                await LoadAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories");
                StatusMessage = "Categories could not be loaded. Review logs or retry refresh.";
            }
        }

        public async Task InitializeAsync()
        {
            if (!_schemaInitialized)
            {
                await _service.EnsureSchemaAsync();
                _schemaInitialized = true;
            }

            if (SelectedInventoryId > 0)
            {
                await _service.EnsureInventoryAsync(SelectedInventoryId, "Main");
            }

            await LoadAsync();
        }

        private Task LoadAsync()
        {
            if (SelectedInventoryId <= 0) return Task.CompletedTask;
            if (IsBusy)
            {
                StatusMessage = "Category refresh is already running.";
                return Task.CompletedTask;
            }

            return LoadCategoryDirectoryAsync();
        }

        private async Task LoadCategoryDirectoryAsync(int? preferredSelectedId = null)
        {
            if (SelectedInventoryId <= 0) return;
            IsBusy = true;
            try
            {
                var selectedId = preferredSelectedId ?? SelectedCategory?.CategoryID;
                var list = await _service.GetCategoriesForInventoryAsync(SelectedInventoryId);
                Categories.Clear();
                foreach (var c in list) Categories.Add(new CategoryItem { CategoryID = c.CategoryID, Name = c.Name });
                ApplyFilter(selectedId);
                StatusMessage = IsCategoryFilterWindowCapped
                    ? $"Loaded {Categories.Count} categories. Showing the first {FilteredCategories.Count} matches so the grid stays responsive."
                    : $"Loaded {Categories.Count} categor{(Categories.Count == 1 ? "y" : "ies")}.";
                _loadFailureDialogShown = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories for inventory {InventoryId}", SelectedInventoryId);
                StatusMessage = Categories.Count == 0
                    ? "Categories could not be loaded. Retry refresh before creating or printing category rows."
                    : "Category refresh failed. Existing category rows were kept so current work can continue.";
                RaiseDirectoryProperties();
                ShowCategoryLoadFailureDialogOnce();
            }
            finally { IsBusy = false; }
        }

        private void ShowCategoryLoadFailureDialogOnce()
        {
            if (_loadFailureDialogShown) return;
            _loadFailureDialogShown = true;
            WpfMessageBox.Show("Categories could not be refreshed. Existing category rows were kept when available; retry refresh or check the application log.", "Category Management", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ClearCategoryStateAfterLoadFailure()
        {
            Categories.Clear();
            FilteredCategories.Clear();
            _matchedCategoryCount = 0;
            _omittedFilteredCategoryCount = 0;
            SelectedCategory = null;
            CategoryName = "";
            RaiseDirectoryProperties();
        }

        private async Task RefreshCategoryDirectoryAfterMutationFailureAsync(int? preferredSelectedId, string refreshedStatusMessage, string clearedStatusMessage)
        {
            if (SelectedInventoryId <= 0)
            {
                ClearCategoryStateAfterLoadFailure();
                StatusMessage = clearedStatusMessage;
                return;
            }

            try
            {
                var list = await _service.GetCategoriesForInventoryAsync(SelectedInventoryId);
                Categories.Clear();
                foreach (var c in list) Categories.Add(new CategoryItem { CategoryID = c.CategoryID, Name = c.Name });
                ApplyFilter(preferredSelectedId);
                StatusMessage = IsCategoryFilterWindowCapped
                    ? $"{refreshedStatusMessage} Showing the first {FilteredCategories.Count} matching rows."
                    : refreshedStatusMessage;
            }
            catch (Exception refreshEx)
            {
                _logger.LogWarning(refreshEx, "Failed to refresh categories after a category mutation failure for inventory {InventoryId}", SelectedInventoryId);
                ClearCategoryStateAfterLoadFailure();
                StatusMessage = clearedStatusMessage;
            }
        }

        private void ApplyFilter(int? preferredSelectedId = null)
        {
            var search = SearchText.Trim();
            var currentId = preferredSelectedId ?? SelectedCategory?.CategoryID;
            var filtered = Categories
                .Where(c => string.IsNullOrWhiteSpace(search)
                    || c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || c.CategoryID.ToString().Contains(search, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.Name)
                .ThenBy(c => c.CategoryID)
                .ToList();

            _matchedCategoryCount = filtered.Count;
            var visible = filtered.Take(MaxVisibleFilteredCategoryRows).ToList();
            _omittedFilteredCategoryCount = Math.Max(0, filtered.Count - visible.Count);
            ReplaceFilteredCategories(visible);

            SelectedCategory = currentId.HasValue
                ? FilteredCategories.FirstOrDefault(c => c.CategoryID == currentId.Value)
                : FilteredCategories.FirstOrDefault();

            RaiseDirectoryProperties();
            RaiseSelectedCategoryProperties();
        }

        private void ReplaceFilteredCategories(IReadOnlyList<CategoryItem> visibleCategories)
        {
            if (FilteredCategories.Count == visibleCategories.Count)
            {
                var unchanged = true;
                for (var i = 0; i < visibleCategories.Count; i++)
                {
                    if (!ReferenceEquals(FilteredCategories[i], visibleCategories[i]))
                    {
                        unchanged = false;
                        break;
                    }
                }

                if (unchanged) return;
            }

            FilteredCategories.Clear();
            foreach (var category in visibleCategories)
                FilteredCategories.Add(category);
        }

        private async Task ClearSearchAsync()
        {
            SearchText = "";
            StatusMessage = "Category filter cleared.";
            await Task.CompletedTask;
        }

        private async Task AddAsync()
        {
            if (SelectedInventoryId <= 0) return;
            var name = CategoryName.Trim();
            if (name.Length == 0) return;

            int? createdCategoryId = null;
            IsBusy = true;
            try
            {
                var id = await _service.EnsureCategoryAsync(name);
                createdCategoryId = id;
                try
                {
                    await _service.LinkCategoryToInventoryAsync(id, SelectedInventoryId);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogInformation(ex, "Category {CategoryId} was already linked to inventory {InventoryId}", id, SelectedInventoryId);
                }

                await LoadCategoryDirectoryAsync(id);
                SelectedCategory = FilteredCategories.FirstOrDefault(x => x.CategoryID == id)
                    ?? Categories.FirstOrDefault(x => x.CategoryID == id);
                StatusMessage = $"Category '{name}' is ready for item assignment.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add category {CategoryName}", name);
                await RefreshCategoryDirectoryAfterMutationFailureAsync(
                    createdCategoryId,
                    $"Category rows were refreshed after '{name}' failed to finish creating.",
                    $"Category rows were cleared after '{name}' failed to finish creating and recovery reload failed.");
                WpfMessageBox.Show($"Category '{name}' could not be created. Category rows were refreshed from the saved data where possible.", "Create Category", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsBusy = false; }
        }

        private async Task SaveAsync()
        {
            if (SelectedCategory == null) return;
            var name = CategoryName.Trim();
            if (name.Length == 0) return;

            var id = SelectedCategory.CategoryID;
            IsBusy = true;
            try
            {
                var ok = await _service.RenameCategoryAsync(id, name);
                if (ok)
                {
                    var item = Categories.FirstOrDefault(x => x.CategoryID == id);
                    if (item != null) item.Name = name;
                    ApplyFilter(id);
                    StatusMessage = $"Category #{id} renamed to '{name}'.";
                }
                else
                {
                    await RefreshCategoryDirectoryAfterMutationFailureAsync(
                        id,
                        $"Category rows were refreshed after category #{id} could not be renamed.",
                        $"Category rows were cleared after category #{id} could not be renamed and recovery reload failed.");
                    WpfMessageBox.Show("The category was not renamed. Category rows were refreshed from the saved data where possible.", "Save Category", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rename category {CategoryId}", id);
                await RefreshCategoryDirectoryAfterMutationFailureAsync(
                    id,
                    $"Category rows were refreshed after category #{id} failed to finish saving.",
                    $"Category rows were cleared after category #{id} failed to finish saving and recovery reload failed.");
                WpfMessageBox.Show("The category could not be saved. Category rows were refreshed from the saved data where possible.", "Save Category", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsBusy = false; }
        }

        private async Task DeleteAsync()
        {
            if (SelectedCategory == null) return;
            var category = SelectedCategory;
            var confirmed = WpfMessageBox.Show(
                $"Delete category \"{category.Name}\"?\n\nOnly delete categories that are no longer needed for item setup or advisor search.",
                "Delete Category",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes;
            if (!confirmed) return;

            IsBusy = true;
            try
            {
                var id = category.CategoryID;
                var ok = await _service.DeleteCategoryAsync(id);
                if (ok)
                {
                    var item = Categories.FirstOrDefault(x => x.CategoryID == id);
                    if (item != null) Categories.Remove(item);
                    CategoryName = "";
                    ApplyFilter();
                    StatusMessage = $"Category '{category.Name}' deleted.";
                }
                else
                {
                    await RefreshCategoryDirectoryAfterMutationFailureAsync(
                        category.CategoryID,
                        $"Category rows were refreshed after '{category.Name}' could not be deleted.",
                        $"Category rows were cleared after '{category.Name}' could not be deleted and recovery reload failed.");
                    WpfMessageBox.Show("The category was not deleted. Category rows were refreshed from the saved data where possible.", "Delete Category", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete category {CategoryId}", category.CategoryID);
                await RefreshCategoryDirectoryAfterMutationFailureAsync(
                    category.CategoryID,
                    $"Category rows were refreshed after '{category.Name}' failed to finish deleting.",
                    $"Category rows were cleared after '{category.Name}' failed to finish deleting and recovery reload failed.");
                WpfMessageBox.Show("The category could not be deleted. Category rows were refreshed from the saved data where possible.", "Delete Category", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsBusy = false; }
        }

        private void RaiseDirectoryProperties()
        {
            OnPropertyChanged(nameof(CategoryResultsSummary));
            OnPropertyChanged(nameof(CategorySetupSummary));
            OnPropertyChanged(nameof(CategoryFilterSummary));
            OnPropertyChanged(nameof(CategoryVisibleWindowSummary));
            OnPropertyChanged(nameof(CategoryPrintSummary));
            OnPropertyChanged(nameof(IsDirectoryPrintAvailable));
            OnPropertyChanged(nameof(IsCategoryEmptyStateVisible));
            OnPropertyChanged(nameof(IsCategoryActionAvailable));
            OnPropertyChanged(nameof(IsSelectedCategoryActionAvailable));
            OnPropertyChanged(nameof(FullFilteredCategoryCount));
            OnPropertyChanged(nameof(FilteredCategoryOmittedCount));
            OnPropertyChanged(nameof(IsCategoryFilterWindowCapped));
            OnPropertyChanged(nameof(CategoryEmptyStateTitle));
            OnPropertyChanged(nameof(CategoryEmptyStateMessage));
            OnPropertyChanged(nameof(CategoryNameStatus));
        }

        private void RaiseSelectedCategoryProperties()
        {
            OnPropertyChanged(nameof(SelectedCategoryTitle));
            OnPropertyChanged(nameof(SelectedCategorySubtitle));
            OnPropertyChanged(nameof(SelectedCategoryDetail));
            OnPropertyChanged(nameof(SelectedCategoryNextAction));
            OnPropertyChanged(nameof(SelectedCategorySummary));
            OnPropertyChanged(nameof(SelectedCategoryChecklist));
            OnPropertyChanged(nameof(SelectedCategoryHandoff));
            OnPropertyChanged(nameof(IsSelectedCategoryActionAvailable));
        }

        private void RaiseCommandStates()
        {
            _addCommand.RaiseCanExecuteChanged();
            _saveCommand.RaiseCanExecuteChanged();
            _deleteCommand.RaiseCanExecuteChanged();
            _refreshCommand.RaiseCanExecuteChanged();
            _clearSearchCommand.RaiseCanExecuteChanged();
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public sealed class CategoryItem : INotifyPropertyChanged
        {
            private int _categoryId;
            private string _name = "";

            public int CategoryID { get => _categoryId; set { if (_categoryId == value) return; _categoryId = value; OnPropertyChanged(); } }
            public string Name { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged(); OnPropertyChanged(nameof(DirectoryLabel)); } }

            public string DirectoryLabel => $"#{CategoryID} | {Name}";

            public event PropertyChangedEventHandler? PropertyChanged;
            private void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
        }

        private sealed class AsyncCommand : ICommand
        {
            private readonly Func<Task> _exec;
            private readonly Func<bool>? _can;
            private bool _running;

            public AsyncCommand(Func<Task> exec, Func<bool>? can = null) { _exec = exec; _can = can; }
            public bool CanExecute(object? parameter) => !_running && (_can?.Invoke() ?? true);
            public event EventHandler? CanExecuteChanged;
            public async void Execute(object? parameter)
            {
                if (!CanExecute(null)) return;
                _running = true; CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                try { await _exec(); }
                finally { _running = false; CanExecuteChanged?.Invoke(this, EventArgs.Empty); }
            }
            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
