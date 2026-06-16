// ViewModels/UsersEditViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Application = System.Windows.Application;
using MediaBrush = System.Windows.Media.Brush;
using MediaBrushes = System.Windows.Media.Brushes;

namespace InventoryManagementApp.ViewModels
{
    public class UsersEditViewModel : ObservableObject
    {
        public string Title { get; }
        public User EditingUser { get; }

        private readonly IFileDialogService _fileDialog;

        public string AccessSummary => EditingUser.AccessSummary;

        public bool CanManageItems { get => Has(User.PermissionManageItems); set => Set(User.PermissionManageItems, value); }
        public bool CanUseRentals { get => Has(User.PermissionRentals); set => Set(User.PermissionRentals, value); }
        public bool CanUseCustomers { get => Has(User.PermissionCustomers); set => Set(User.PermissionCustomers, value); }
        public bool CanUseMaintenance { get => Has(User.PermissionMaintenance); set => Set(User.PermissionMaintenance, value); }
        public bool CanUseCalibration { get => Has(User.PermissionCalibration); set => Set(User.PermissionCalibration, value); }
        public bool CanUseReservations { get => Has(User.PermissionReservations); set => Set(User.PermissionReservations, value); }
        public bool CanUseKits { get => Has(User.PermissionKits); set => Set(User.PermissionKits, value); }
        public bool CanUseCategories { get => Has(User.PermissionCategories); set => Set(User.PermissionCategories, value); }
        public bool CanPrintLabels { get => Has(User.PermissionPrintLabels); set => Set(User.PermissionPrintLabels, value); }
        public bool CanUseReports { get => Has(User.PermissionReports); set => Set(User.PermissionReports, value); }
        public bool CanUseActivityLogs { get => Has(User.PermissionActivityLogs); set => Set(User.PermissionActivityLogs, value); }
        public bool CanUseImportExport { get => Has(User.PermissionImportExport); set => Set(User.PermissionImportExport, value); }
        public bool CanManageUsers { get => Has(User.PermissionManageUsers); set => Set(User.PermissionManageUsers, value); }
        public bool CanUseSettings { get => Has(User.PermissionSettings); set => Set(User.PermissionSettings, value); }

        public IRelayCommand BrowseImageCommand { get; }
        public IRelayCommand RemoveImageCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IRelayCommand SelectAdvisorPresetCommand { get; }
        public IRelayCommand SelectTechnicianPresetCommand { get; }
        public IRelayCommand SelectAdminPresetCommand { get; }
        public IRelayCommand ClearPermissionsCommand { get; }

        public UsersEditViewModel(User user, IFileDialogService fileDialog, Func<Task> onSave, Action onCancel)
        {
            EditingUser = user ?? new User();
            EditingUser.PropertyChanged += EditingUser_PropertyChanged;
            _fileDialog = fileDialog;
            Title = (EditingUser.UserID == 0) ? "Add User" : "Edit User";
            BrowseImageCommand = new RelayCommand(BrowseImage);
            RemoveImageCommand = new RelayCommand(RemoveImage);
            SaveCommand = new AsyncRelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);
            SelectAdvisorPresetCommand = new RelayCommand(() => ApplyPreset(
                User.PermissionRentals,
                User.PermissionCustomers,
                User.PermissionReservations,
                User.PermissionKits,
                User.PermissionPrintLabels,
                User.PermissionReports));
            SelectTechnicianPresetCommand = new RelayCommand(() => ApplyPreset(
                User.PermissionRentals,
                User.PermissionMaintenance,
                User.PermissionCalibration,
                User.PermissionKits,
                User.PermissionCategories,
                User.PermissionPrintLabels));
            SelectAdminPresetCommand = new RelayCommand(() =>
            {
                EditingUser.IsAdmin = true;
                NotifyAllPermissionProperties();
            });
            ClearPermissionsCommand = new RelayCommand(() => ApplyPreset(Array.Empty<string>()));
        }

        void EditingUser_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(User.IsAdmin) || e.PropertyName == nameof(User.Permissions))
                NotifyAllPermissionProperties();
        }

        bool Has(string permissionKey) => EditingUser.HasPermission(permissionKey);

        void Set(string permissionKey, bool allowed)
        {
            EditingUser.SetPermission(permissionKey, allowed);
            OnPropertyChanged(nameof(AccessSummary));
        }

        void ApplyPreset(params string[] permissionKeys)
        {
            EditingUser.IsAdmin = false;
            EditingUser.Permissions = User.BuildPermissions(permissionKeys);
            NotifyAllPermissionProperties();
        }

        void NotifyAllPermissionProperties()
        {
            OnPropertyChanged(nameof(CanManageItems));
            OnPropertyChanged(nameof(CanUseRentals));
            OnPropertyChanged(nameof(CanUseCustomers));
            OnPropertyChanged(nameof(CanUseMaintenance));
            OnPropertyChanged(nameof(CanUseCalibration));
            OnPropertyChanged(nameof(CanUseReservations));
            OnPropertyChanged(nameof(CanUseKits));
            OnPropertyChanged(nameof(CanUseCategories));
            OnPropertyChanged(nameof(CanPrintLabels));
            OnPropertyChanged(nameof(CanUseReports));
            OnPropertyChanged(nameof(CanUseActivityLogs));
            OnPropertyChanged(nameof(CanUseImportExport));
            OnPropertyChanged(nameof(CanManageUsers));
            OnPropertyChanged(nameof(CanUseSettings));
            OnPropertyChanged(nameof(AccessSummary));
        }

        void BrowseImage()
        {
            var path = _fileDialog.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var fullPath = Path.GetFullPath(path);
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var targetPath = fullPath;

            if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            {
                var assetsDir = Path.Combine(baseDir, "Assets", "UserPhotos");
                Directory.CreateDirectory(assetsDir);
                var fileName = Path.GetFileName(fullPath);
                targetPath = Path.Combine(assetsDir, fileName);
                File.Copy(fullPath, targetPath, true);
            }

            var relativePath = Path.GetRelativePath(baseDir, targetPath);
            EditingUser.UserPhotoPath = relativePath;
        }

        void RemoveImage()
        {
            EditingUser.UserPhotoPath = string.Empty;
            var brush = Application.Current?.TryFindResource("ForegroundBrush") as MediaBrush;
            EditingUser.InitialsBrush = brush ?? MediaBrushes.Black;
        }
    }
}
