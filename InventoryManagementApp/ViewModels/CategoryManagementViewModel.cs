// ViewModels/CategoryManagementViewModel.cs
using System;
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
        private readonly CategoriesService _service;
        private readonly ILogger<CategoryManagementViewModel> _logger;
        private int _selectedInventoryId;
        private CategoryItem? _selectedCategory;
        private string _categoryName = "";
        private string _searchText = "";
        private string _statusMessage = "Ready to manage category setup.";
        private bool _isBusy;

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
                _addCommand.RaiseCanExecuteChanged();
                LoadCategoriesAsync();
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
                _saveCommand.RaiseCanExecuteChanged();
                _deleteCommand.RaiseCanExecuteChanged();
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
                _addCommand.RaiseCanExecuteChanged();
                _saveCommand.RaiseCanExecuteChanged();
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
                _clearSearchCommand.RaiseCanExecuteChanged();
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
            }
        }

        public string CategoryResultsSummary => string.IsNullOrWhiteSpace(SearchText)
            ? $"{FilteredCategories.Count} {(FilteredCategories.Count == 1 ? "category" : "categories")} shown"
            : $"{FilteredCategories.Count} of {Categories.Count} categor{(FilteredCategories.Count == 1 ? "y" : "ies")} match \"{SearchText.Trim()}\"";

        public string CategorySetupSummary => Categories.Count == 0
            ? "No categories have been linked to this inventory area yet."
            : $"{Categories.Count} categor{(Categories.Count == 1 ? "y" : "ies")} support filtering, item setup, and advisor search.";

        public string CategoryFilterSummary => string.IsNullOrWhiteSpace(SearchText)
            ? "Showing every linked category."
            : $"Filter active: \"{SearchText.Trim()}\".";

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
            _addCommand = new AsyncCommand(AddAsync, () => !string.IsNullOrWhiteSpace(CategoryName) && SelectedInventoryId > 0);
            _saveCommand = new AsyncCommand(SaveAsync, () => SelectedCategory != null && !string.IsNullOrWhiteSpace(CategoryName));
            _deleteCommand = new AsyncCommand(DeleteAsync, () => SelectedCategory != null);
            _refreshCommand = new AsyncCommand(LoadAsync);
            _clearSearchCommand = new AsyncCommand(ClearSearchAsync, () => !string.IsNullOrWhiteSpace(SearchText));
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
            await _service.EnsureSchemaAsync();
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (SelectedInventoryId <= 0) return;
            IsBusy = true;
            try
            {
                var selectedId = SelectedCategory?.CategoryID;
                var list = await _service.GetCategoriesForInventoryAsync(SelectedInventoryId);
                Categories.Clear();
                foreach (var c in list) Categories.Add(new CategoryItem { CategoryID = c.CategoryID, Name = c.Name });
                ApplyFilter(selectedId);
                StatusMessage = $"Loaded {Categories.Count} categor{(Categories.Count == 1 ? "y" : "ies")}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories for inventory {InventoryId}", SelectedInventoryId);
                StatusMessage = "Categories could not be loaded. Review logs or retry refresh.";
                WpfMessageBox.Show("Categories could not be loaded. Please retry or check the application log.", "Category Management", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsBusy = false; }
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

            FilteredCategories.Clear();
            foreach (var category in filtered)
                FilteredCategories.Add(category);

            SelectedCategory = currentId.HasValue
                ? FilteredCategories.FirstOrDefault(c => c.CategoryID == currentId.Value)
                : FilteredCategories.FirstOrDefault();

            RaiseDirectoryProperties();
        }

        private async Task ClearSearchAsync()
        {
            SearchText = "";
            ApplyFilter();
            StatusMessage = "Category filter cleared.";
            await Task.CompletedTask;
        }

        private async Task AddAsync()
        {
            if (SelectedInventoryId <= 0) return;
            var name = CategoryName.Trim();
            if (name.Length == 0) return;

            IsBusy = true;
            try
            {
                var id = await _service.EnsureCategoryAsync(name);
                try
                {
                    await _service.LinkCategoryToInventoryAsync(id, SelectedInventoryId);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogInformation(ex, "Category {CategoryId} was already linked to inventory {InventoryId}", id, SelectedInventoryId);
                }

                await LoadAsync();
                SelectedCategory = FilteredCategories.FirstOrDefault(x => x.CategoryID == id)
                    ?? Categories.FirstOrDefault(x => x.CategoryID == id);
                StatusMessage = $"Category '{name}' is ready for item assignment.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add category {CategoryName}", name);
                StatusMessage = $"Category '{name}' could not be created.";
                WpfMessageBox.Show($"Category '{name}' could not be created. Please retry or check the application log.", "Create Category", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    StatusMessage = $"Category #{id} could not be renamed.";
                    WpfMessageBox.Show("The category was not renamed. Refresh and try again.", "Save Category", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rename category {CategoryId}", id);
                StatusMessage = $"Category #{id} could not be renamed.";
                WpfMessageBox.Show("The category could not be saved. Please retry or check the application log.", "Save Category", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    StatusMessage = $"Category '{category.Name}' could not be deleted.";
                    WpfMessageBox.Show("The category was not deleted. Refresh and try again.", "Delete Category", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete category {CategoryId}", category.CategoryID);
                StatusMessage = $"Category '{category.Name}' could not be deleted.";
                WpfMessageBox.Show("The category could not be deleted. It may still be needed by other records.", "Delete Category", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { IsBusy = false; }
        }

        private void RaiseDirectoryProperties()
        {
            OnPropertyChanged(nameof(CategoryResultsSummary));
            OnPropertyChanged(nameof(CategorySetupSummary));
            OnPropertyChanged(nameof(CategoryFilterSummary));
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
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public sealed class CategoryItem : INotifyPropertyChanged
        {
            private int _categoryId;
            private string _name = "";

            public int CategoryID { get => _categoryId; set { if (_categoryId == value) return; _categoryId = value; OnPropertyChanged(); } }
            public string Name { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged(); } }

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
