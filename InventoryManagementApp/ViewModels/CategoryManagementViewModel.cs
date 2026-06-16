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
                OnPropertyChanged(nameof(SelectedCategoryTitle));
                OnPropertyChanged(nameof(SelectedCategorySubtitle));
                OnPropertyChanged(nameof(SelectedCategoryDetail));
                OnPropertyChanged(nameof(SelectedCategoryNextAction));
                OnPropertyChanged(nameof(SelectedCategorySummary));
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

        public bool IsBusy
        {
            get => _isBusy;
            private set { if (_isBusy == value) return; _isBusy = value; OnPropertyChanged(); }
        }

        public string CategoryResultsSummary => string.IsNullOrWhiteSpace(SearchText)
            ? $"{FilteredCategories.Count} {(FilteredCategories.Count == 1 ? "category" : "categories")} shown"
            : $"{FilteredCategories.Count} of {Categories.Count} categor{(FilteredCategories.Count == 1 ? "y" : "ies")} match \"{SearchText.Trim()}\"";

        public string SelectedCategoryTitle => SelectedCategory == null
            ? "No category selected"
            : SelectedCategory.Name;

        public string SelectedCategorySubtitle => SelectedCategory == null
            ? "Select or double-click a category row."
            : $"Category #{SelectedCategory.CategoryID}";

        public string SelectedCategoryDetail => SelectedCategory == null
            ? "Choose a category to rename, delete, copy, print, or review before assigning inventory records."
            : $"Category #{SelectedCategory.CategoryID} is named \"{SelectedCategory.Name}\". Use this directory to keep advisor search filters and inventory setup tidy.";

        public string SelectedCategoryNextAction => SelectedCategory == null
            ? "Create a category, filter the list, or select a row to continue."
            : "Natural next step: confirm the name matches how technicians and advisors search, then use inventory item setup to assign matching records.";

        public string SelectedCategorySummary => SelectedCategory == null
            ? "Select or double-click a category row to view details, copy it, print the directory, rename it, or delete it."
            : $"Selected: #{SelectedCategory.CategoryID} | {SelectedCategory.Name}";

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

            OnPropertyChanged(nameof(CategoryResultsSummary));
        }

        private async Task ClearSearchAsync()
        {
            SearchText = "";
            ApplyFilter();
            await Task.CompletedTask;
        }

        private async Task AddAsync()
        {
            if (SelectedInventoryId <= 0) return;
            var id = await _service.EnsureCategoryAsync(CategoryName.Trim());
            try
            {
                await _service.LinkCategoryToInventoryAsync(id, SelectedInventoryId);
            }
            catch (InvalidOperationException)
            {
                return;
            }
            await LoadAsync();
            SelectedCategory = FilteredCategories.FirstOrDefault(x => x.CategoryID == id)
                ?? Categories.FirstOrDefault(x => x.CategoryID == id);
        }

        private async Task SaveAsync()
        {
            if (SelectedCategory == null) return;
            var name = CategoryName.Trim();
            if (name.Length == 0) return;
            var ok = await _service.RenameCategoryAsync(SelectedCategory.CategoryID, name);
            if (ok)
            {
                var item = Categories.FirstOrDefault(x => x.CategoryID == SelectedCategory.CategoryID);
                if (item != null) item.Name = name;
                ApplyFilter(SelectedCategory.CategoryID);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedCategory == null) return;
            var category = SelectedCategory;
            var confirmed = WpfMessageBox.Show(
                $"Delete category \"{category.Name}\"?",
                "Delete Category",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes;
            if (!confirmed) return;

            var id = category.CategoryID;
            var ok = await _service.DeleteCategoryAsync(id);
            if (ok)
            {
                var item = Categories.FirstOrDefault(x => x.CategoryID == id);
                if (item != null) Categories.Remove(item);
                CategoryName = "";
                ApplyFilter();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public sealed class CategoryItem : INotifyPropertyChanged
        {
            private int _categoryId;
            private string _name = "";

            public int CategoryID { get => _categoryId; set { if (_categoryId == value) return; _categoryId = value; OnPropertyChanged(); } }
            public string Name { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged(); } }

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
