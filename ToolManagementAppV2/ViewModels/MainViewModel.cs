// File: ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        // Selected items (for master/detail)
        public Tool SelectedTool { get; set; }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                OnPropertyChanged(nameof(IsLastAdmin));
            }
        }

        // New properties for password change
        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }
        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        // Tool Management
        public string SearchTerm { get; set; }

        // Commands (omitted for brevity)
        public ICommand SearchCommand { get; }
        public ICommand AddToolCommand { get; }
        public ICommand UpdateToolCommand { get; }
        public ICommand DeleteToolCommand { get; }
        public ICommand LoadUsersCommand { get; }

        // Header Logo property
        private BitmapImage _headerLogo;
        public BitmapImage HeaderLogo
        {
            get
            {
                if (_headerLogo == null)
                {
                    string logoPath = _settingsService.GetSetting("CompanyLogoPath");
                    if (string.IsNullOrEmpty(logoPath) || !System.IO.File.Exists(logoPath))
                    {
                        _headerLogo = new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultLogo.png", UriKind.Absolute));
                    }
                    else
                    {
                        _headerLogo = new BitmapImage(new Uri(logoPath, UriKind.Absolute));
                    }
                }
                return _headerLogo;
            }
            set => SetProperty(ref _headerLogo, value);
        }

        public bool IsLastAdmin
        {
            get
            {
                if (SelectedUser == null)
                    return false;
                if (!SelectedUser.IsAdmin)
                    return false;
                return Users.Count(u => u.IsAdmin) == 1;
            }
        }

        public MainViewModel(ToolService toolService, UserService userService, SettingsService settingsService)
        {
            _toolService = toolService;
            _userService = userService;
            _settingsService = settingsService;
            SearchCommand = new RelayCommand(SearchTools);
            AddToolCommand = new RelayCommand(AddTool);
            UpdateToolCommand = new RelayCommand(UpdateTool);
            DeleteToolCommand = new RelayCommand(DeleteTool);
            LoadUsersCommand = new RelayCommand(LoadUsers);

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

        public void LoadUsers()
        {
            Users.Clear();
            var usersList = _userService.GetAllUsers();
            foreach (var user in usersList)
            {
                if (!string.IsNullOrEmpty(user.UserPhotoPath) && System.IO.File.Exists(user.UserPhotoPath))
                {
                    user.PhotoBitmap = new BitmapImage(new Uri(user.UserPhotoPath, UriKind.Absolute))
                    {
                        CacheOption = BitmapCacheOption.OnLoad
                    };
                }
                Users.Add(user);
            }
            // Do not auto-select a user; user details remain empty until one is selected.
            SelectedUser = null;
            OnPropertyChanged(nameof(Users));
            OnPropertyChanged(nameof(SelectedUser));
            OnPropertyChanged(nameof(IsLastAdmin));
        }

        public void LoadCurrentUser()
        {
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
            CurrentUserName = "Guest";
            CurrentUserPhoto = LoadPhotoBitmap(null);
        }

        private BitmapImage LoadPhotoBitmap(string photoPath)
        {
            if (string.IsNullOrEmpty(photoPath) || !System.IO.File.Exists(photoPath))
            {
                var defaultPhotoUri = new Uri("pack://application:,,,/Resources/DefaultUserPhoto.png", UriKind.Absolute);
                return new BitmapImage(defaultPhotoUri) { CacheOption = BitmapCacheOption.OnLoad };
            }
            return new BitmapImage(new Uri(photoPath, UriKind.Absolute)) { CacheOption = BitmapCacheOption.OnLoad };
        }

        public void ClearUserInputFields()
        {
            SelectedUser = new User();
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

        private string _userPassword;
        public string UserPassword
        {
            get => _userPassword;
            set => SetProperty(ref _userPassword, value);
        }
    }
}
