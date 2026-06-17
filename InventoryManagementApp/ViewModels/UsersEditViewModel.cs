// ViewModels/UsersEditViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        public string PermissionStatusSummary => EditingUser.IsAdmin
            ? "Full administrator: this user can see every section and use every admin-level action."
            : GetAllowedPermissionLabels().Any()
                ? $"Custom access: {EditingUser.UserName} can see {GetAllowedPermissionLabels().Count()} section{(GetAllowedPermissionLabels().Count() == 1 ? string.Empty : "s")}."
                : "No access assigned: this user can sign in only if they are active, but no app sections will be available.";

        public string AllowedPermissionSummary => EditingUser.IsAdmin
            ? "Every app section, setting, and guarded action is available."
            : BuildPermissionSummary(GetAllowedPermissionLabels(), "No app sections assigned.");

        public string HiddenPermissionSummary => EditingUser.IsAdmin
            ? "Nothing is hidden while Full administrator is ticked."
            : BuildPermissionSummary(GetHiddenPermissionLabels(), "Nothing hidden for the selected permissions.");

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
            if (e.PropertyName == nameof(User.IsAdmin) || e.PropertyName == nameof(User.Permissions) || e.PropertyName == nameof(User.UserName))
                NotifyAllPermissionProperties();
        }

        bool Has(string permissionKey) => EditingUser.HasPermission(permissionKey);

        void Set(string permissionKey, bool allowed)
        {
            EditingUser.SetPermission(permissionKey, allowed);
            NotifyAllPermissionProperties();
        }

        void ApplyPreset(params string[] permissionKeys)
        {
            EditingUser.IsAdmin = false;
            EditingUser.Permissions = User.BuildPermissions(permissionKeys);
            NotifyAllPermissionProperties();
        }

        IEnumerable<string> GetAllowedPermissionLabels()
        {
            if (EditingUser.IsAdmin)
                return User.PermissionLabels.Values;

            return User.PermissionLabels
                .Where(permission => EditingUser.HasPermission(permission.Key))
                .Select(permission => permission.Value);
        }

        IEnumerable<string> GetHiddenPermissionLabels()
            => User.PermissionLabels
                .Where(permission => !EditingUser.HasPermission(permission.Key))
                .Select(permission => permission.Value);

        static string BuildPermissionSummary(IEnumerable<string> labels, string emptyText)
        {
            var list = labels.ToList();
            return list.Count == 0 ? emptyText : string.Join(", ", list);
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
            OnPropertyChanged(nameof(PermissionStatusSummary));
            OnPropertyChanged(nameof(AllowedPermissionSummary));
            OnPropertyChanged(nameof(HiddenPermissionSummary));
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