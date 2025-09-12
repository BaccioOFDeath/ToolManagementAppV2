// ViewModels/CategoryManagementViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using InventoryManagementApp.Services.Categories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.ViewModels
{
    public sealed class CategoryManagementViewModel : INotifyPropertyChanged
    {
        private readonly CategoriesService _service;
        private readonly ILogger<CategoryManagementViewModel> _logger;
        private int _selectedInventoryId;
        private CategoryItem? _selectedCategory;
        private string _categoryName = "";
        private bool _isBusy;

        public ObservableCollection<CategoryItem> Categories { get; } = new();

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

        public bool IsBusy
        {
            get => _isBusy;
            private set { if (_isBusy == value) return; _isBusy = value; OnPropertyChanged(); }
        }

        private readonly AsyncCommand _addCommand;
        private readonly AsyncCommand _saveCommand;
        private readonly AsyncCommand _deleteCommand;
        private readonly AsyncCommand _refreshCommand;

        public ICommand AddCommand => _addCommand;
        public ICommand SaveCommand => _saveCommand;
        public ICommand DeleteCommand => _deleteCommand;
        public ICommand RefreshCommand => _refreshCommand;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CategoryManagementViewModel(CategoriesService service, ILogger<CategoryManagementViewModel>? logger = null)
        {
            _service = service;
            _logger = logger ?? NullLogger<CategoryManagementViewModel>.Instance;
            _addCommand = new AsyncCommand(AddAsync, () => !string.IsNullOrWhiteSpace(CategoryName) && SelectedInventoryId > 0);
            _saveCommand = new AsyncCommand(SaveAsync, () => SelectedCategory != null && !string.IsNullOrWhiteSpace(CategoryName));
            _deleteCommand = new AsyncCommand(DeleteAsync, () => SelectedCategory != null);
            _refreshCommand = new AsyncCommand(LoadAsync);
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
                var list = await _service.GetCategoriesForInventoryAsync(SelectedInventoryId);
                Categories.Clear();
                foreach (var c in list) Categories.Add(new CategoryItem { CategoryID = c.CategoryID, Name = c.Name });
                if (SelectedCategory != null)
                {
                    var match = Categories.FirstOrDefault(x => x.CategoryID == SelectedCategory.CategoryID);
                    SelectedCategory = match;
                }
            }
            finally { IsBusy = false; }
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
            SelectedCategory = Categories.FirstOrDefault(x => x.CategoryID == id);
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
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedCategory == null) return;
            var id = SelectedCategory.CategoryID;
            var ok = await _service.DeleteCategoryAsync(id);
            if (ok)
            {
                var idx = Categories.IndexOf(Categories.First(x => x.CategoryID == id));
                if (idx >= 0) Categories.RemoveAt(idx);
                CategoryName = "";
                SelectedCategory = null;
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
