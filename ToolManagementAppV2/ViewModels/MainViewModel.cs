// File: ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
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

        public ICommand SearchCommand { get; }
        public ICommand AddToolCommand { get; }
        public ICommand UpdateToolCommand { get; }
        public ICommand DeleteToolCommand { get; }
        public ICommand LoadUsersCommand { get; }

        public ICommand ChooseProfilePicCommand { get; }
        public ICommand UploadUserPhotoCommand { get; }

        public ObservableCollection<Tool> Tools { get; } = new();
        public ObservableCollection<Tool> SearchResults { get; } = new();
        public ObservableCollection<Tool> CheckedOutTools { get; } = new();
        public ObservableCollection<User> Users { get; } = new();

        private Tool _selectedTool;
        public Tool SelectedTool
        {
            get => _selectedTool;
            set => SetProperty(ref _selectedTool, value);
        }

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

        private BitmapImage _headerLogo;
        public BitmapImage HeaderLogo
        {
            get
            {
                if (_headerLogo == null)
                {
                    var path = _settingsService.GetSetting("CompanyLogoPath");
                    _headerLogo = new BitmapImage(new Uri(
                        string.IsNullOrEmpty(path) || !File.Exists(path)
                            ? "pack://application:,,,/Resources/DefaultLogo.png"
                            : path,
                        UriKind.Absolute));
                }
                return _headerLogo;
            }
            set => SetProperty(ref _headerLogo, value);
        }

        // password/change properties
        private string _userPassword;
        public string UserPassword
        {
            get => _userPassword;
            set => SetProperty(ref _userPassword, value);
        }

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

        public string SearchTerm { get; set; }

        public bool IsLastAdmin =>
            SelectedUser != null &&
            SelectedUser.IsAdmin &&
            Users.Count(u => u.IsAdmin) == 1;

        public MainViewModel(
            ToolService toolService,
            UserService userService,
            SettingsService settingsService)
        {
            _toolService = toolService;
            _userService = userService;
            _settingsService = settingsService;

            SearchCommand = new RelayCommand(SearchTools);
            AddToolCommand = new RelayCommand(AddTool);
            UpdateToolCommand = new RelayCommand(UpdateTool);
            DeleteToolCommand = new RelayCommand(DeleteTool);
            LoadUsersCommand = new RelayCommand(LoadUsers);

            ChooseProfilePicCommand = new RelayCommand(ChooseProfilePic);
            UploadUserPhotoCommand = new RelayCommand(UploadSelectedUserPhoto);

            LoadTools();
            LoadCheckedOutTools();
            LoadUsers();
            LoadCurrentUser();
        }

        public void LoadUsers()
        {
            Users.Clear();
            foreach (var u in _userService.GetAllUsers())
                Users.Add(u);
            SelectedUser = null;
            OnPropertyChanged(nameof(IsLastAdmin));
        }

        public void LoadCurrentUser()
        {
            if (Application.Current.Properties["CurrentUser"] is User curr)
            {
                CurrentUserName = curr.UserName;
                CurrentUserPhoto = File.Exists(curr.UserPhotoPath)
                    ? new BitmapImage(new Uri(curr.UserPhotoPath, UriKind.Absolute))
                    : new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultUserPhoto.png", UriKind.Absolute));
            }
        }

        private void LoadTools()
        {
            Tools.Clear();
            foreach (var t in _toolService.GetAllTools())
                Tools.Add(t);
        }

        private void LoadCheckedOutTools()
        {
            CheckedOutTools.Clear();
            foreach (var t in _toolService.GetAllTools().Where(t => t.IsCheckedOut))
                CheckedOutTools.Add(t);
        }

        private void SearchTools()
        {
            SearchResults.Clear();
            foreach (var t in _toolService.SearchTools(SearchTerm))
                SearchResults.Add(t);
        }

        private void AddTool()
        {
            var nt = new Tool { ToolID = "NewID", Description = "New Description" };
            _toolService.AddTool(nt);
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

        private void ChooseProfilePic()
        {
            if (Application.Current.Properties["CurrentUser"] is User cu)
                UploadPhotoForUser(cu);
        }

        private void UploadSelectedUserPhoto()
        {
            if (SelectedUser != null)
                UploadPhotoForUser(SelectedUser);
        }

        private void UploadPhotoForUser(User u)
        {
            var dlg = new OpenFileDialog { Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg" };
            if (dlg.ShowDialog() != true) return;

            var src = dlg.FileName;
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserPhotos");
            Directory.CreateDirectory(folder);
            var dest = Path.Combine(folder, $"{Guid.NewGuid()}{Path.GetExtension(src)}");
            File.Copy(src, dest, true);

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(dest, UriKind.Absolute);
            bmp.EndInit();

            u.UserPhotoPath = dest;
            u.PhotoBitmap = bmp;
            _userService.UpdateUser(u);

            if (Application.Current.Properties["CurrentUser"] is User cu && cu.UserID == u.UserID)
            {
                cu.UserPhotoPath = dest;
                cu.PhotoBitmap = bmp;
                CurrentUserPhoto = bmp;
                CurrentUserName = cu.UserName;
            }

            LoadUsers();
        }
    }
}
