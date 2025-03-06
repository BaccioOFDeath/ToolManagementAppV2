using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Services;

namespace ToolManagementAppV2.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ToolService _toolService;
        private readonly UserService _userService;
        private readonly SettingsService _settingsService;

        // Properties for Current User
        private string _currentUserName;
        public string CurrentUserName
        {
            get => _currentUserName;
            set => SetProperty(ref _currentUserName, value);
        }

        private BitmapImage _currentUserPhoto;
        public BitmapImage CurrentUserPhoto
        {
            get => _currentUserPhoto;
            set => SetProperty(ref _currentUserPhoto, value);
        }

        // Observable Collections
        public ObservableCollection<Tool> Tools { get; } = new();
        public ObservableCollection<Tool> SearchResults { get; } = new();
        public ObservableCollection<Tool> CheckedOutTools { get; } = new();
        public ObservableCollection<User> Users { get; } = new();

        // Tool Management
        public string SearchTerm { get; set; }
        public Tool SelectedTool { get; set; }

        // Commands
        public ICommand SearchCommand { get; }
        public ICommand AddToolCommand { get; }
        public ICommand UpdateToolCommand { get; }
        public ICommand DeleteToolCommand { get; }
        public ICommand LoadUsersCommand { get; }

        public MainViewModel(ToolService toolService, UserService userService, SettingsService settingsService)
        {
            _toolService = toolService;
            _userService = userService;
            _settingsService = settingsService;

            // Initialize Commands
            SearchCommand = new RelayCommand(SearchTools);
            AddToolCommand = new RelayCommand(AddTool);
            UpdateToolCommand = new RelayCommand(UpdateTool);
            DeleteToolCommand = new RelayCommand(DeleteTool);
            LoadUsersCommand = new RelayCommand(LoadUsers);

            // Load Initial Data
            LoadTools();
            LoadCheckedOutTools();
            LoadUsers();
            LoadCurrentUser();
        }

        private void LoadTools()
        {
            Tools.Clear();
            var allTools = _toolService.GetAllTools();
            foreach (var tool in allTools)
                Tools.Add(tool);
        }

        private void LoadCheckedOutTools()
        {
            CheckedOutTools.Clear();
            var allTools = _toolService.GetAllTools();
            var checkedOut = allTools.Where(t => t.IsCheckedOut);
            foreach (var tool in checkedOut)
                CheckedOutTools.Add(tool);
        }

        private void SearchTools()
        {
            SearchResults.Clear();
            var results = _toolService.SearchTools(SearchTerm);
            foreach (var tool in results)
                SearchResults.Add(tool);
        }

        private void AddTool()
        {
            // Example: Replace with proper input values
            var newTool = new Tool { ToolID = "NewID", Description = "New Description" };
            _toolService.AddTool(newTool);
            LoadTools();
        }

        private void UpdateTool()
        {
            if (SelectedTool != null)
            {
                _toolService.UpdateTool(SelectedTool);
                LoadTools();
            }
        }

        private void DeleteTool()
        {
            if (SelectedTool != null)
            {
                _toolService.DeleteTool(SelectedTool.ToolID);
                LoadTools();
            }
        }

        private void LoadUsers()
        {
            Users.Clear();
            var usersList = _userService.GetAllUsers();
            foreach (var user in usersList)
                Users.Add(user);
        }

        private void LoadCurrentUser()
        {
            // Retrieve current user from App.Current.Properties if available.
            if (Application.Current.Properties.Contains("CurrentUser"))
            {
                var currentUser = Application.Current.Properties["CurrentUser"] as User;
                if (currentUser != null)
                {
                    CurrentUserName = currentUser.UserName;
                    CurrentUserPhoto = LoadPhotoBitmap(currentUser.UserPhotoPath);
                    return;
                }
            }
            // Default to Guest if not found.
            CurrentUserName = "Guest";
            CurrentUserPhoto = LoadPhotoBitmap(null);
        }

        private BitmapImage LoadPhotoBitmap(string photoPath)
        {
            if (string.IsNullOrEmpty(photoPath) || !System.IO.File.Exists(photoPath))
            {
                // Attempt to load a default user photo from the app directory.
                var defaultPhotoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DefaultUserPhoto.png");
                if (System.IO.File.Exists(defaultPhotoPath))
                {
                    return new BitmapImage(new Uri(defaultPhotoPath, System.UriKind.Absolute))
                    {
                        CacheOption = BitmapCacheOption.OnLoad
                    };
                }
                else
                {
                    MessageBox.Show("DefaultUserPhoto.png not found in the application directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
            }

            return new BitmapImage(new Uri(photoPath, System.UriKind.Absolute))
            {
                CacheOption = BitmapCacheOption.OnLoad
            };
        }
    }
}
