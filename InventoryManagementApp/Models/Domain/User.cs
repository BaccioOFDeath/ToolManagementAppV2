using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using MediaBrush = System.Windows.Media.Brush;
using MediaBrushes = System.Windows.Media.Brushes;

#nullable enable

namespace InventoryManagementApp.Models.Domain
{
    public class User : ObservableObject
    {
        public const string PermissionManageItems = "manage-items";
        public const string PermissionRentals = "rentals";
        public const string PermissionCustomers = "customers";
        public const string PermissionMaintenance = "maintenance";
        public const string PermissionCalibration = "calibration";
        public const string PermissionReservations = "reservations";
        public const string PermissionKits = "kits";
        public const string PermissionCategories = "categories";
        public const string PermissionPrintLabels = "print-labels";
        public const string PermissionReports = "reports";
        public const string PermissionActivityLogs = "activity-logs";
        public const string PermissionImportExport = "import-export";
        public const string PermissionManageUsers = "manage-users";
        public const string PermissionSettings = "settings";

        public static readonly IReadOnlyDictionary<string, string> PermissionLabels = new Dictionary<string, string>
        {
            [PermissionManageItems] = "Manage items",
            [PermissionRentals] = "Rentals / checkout",
            [PermissionCustomers] = "Customers",
            [PermissionMaintenance] = "Maintenance",
            [PermissionCalibration] = "Calibration",
            [PermissionReservations] = "Reservations / holds",
            [PermissionKits] = "Kits",
            [PermissionCategories] = "Categories",
            [PermissionPrintLabels] = "Print labels",
            [PermissionReports] = "Reports",
            [PermissionActivityLogs] = "Activity logs",
            [PermissionImportExport] = "Import / export",
            [PermissionManageUsers] = "Manage users",
            [PermissionSettings] = "Settings"
        };

        public static readonly IReadOnlyCollection<string> DefaultUserPermissions = new[]
        {
            PermissionRentals,
            PermissionCustomers,
            PermissionMaintenance,
            PermissionCalibration,
            PermissionReservations,
            PermissionKits,
            PermissionCategories,
            PermissionPrintLabels,
            PermissionReports,
            PermissionActivityLogs
        };

        const string NoPermissionsValue = "none";

        private int _userID;
        public int UserID { get => _userID; set => SetProperty(ref _userID, value); }

        private string _userName = string.Empty;
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

        private string _passwordHash = string.Empty;
        public string PasswordHash { get => _passwordHash; set => SetProperty(ref _passwordHash, value); }

        private string _passwordSalt = string.Empty;
        public string PasswordSalt { get => _passwordSalt; set => SetProperty(ref _passwordSalt, value); }

        private string _userPhotoPath = string.Empty;
        public string UserPhotoPath { get => _userPhotoPath; set => SetProperty(ref _userPhotoPath, value); }

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                if (SetProperty(ref _isAdmin, value))
                    OnPropertyChanged(nameof(AccessSummary));
            }
        }

        private string _email = string.Empty;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _phone = string.Empty;
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }

        private string _mobile = string.Empty;
        public string Mobile { get => _mobile; set => SetProperty(ref _mobile, value); }

        private string _address = string.Empty;
        public string Address { get => _address; set => SetProperty(ref _address, value); }

        private string _role = string.Empty;
        public string Role { get => _role; set => SetProperty(ref _role, value); }

        private bool _isActive = true;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

        private DateTime? _createdAt;
        public DateTime? CreatedAt { get => _createdAt; set => SetProperty(ref _createdAt, value); }

        private bool _passwordExpired;
        public bool PasswordExpired { get => _passwordExpired; set => SetProperty(ref _passwordExpired, value); }

        private int _failedLoginAttempts;
        public int FailedLoginAttempts { get => _failedLoginAttempts; set => SetProperty(ref _failedLoginAttempts, value); }

        private DateTime? _lockoutEndUtc;
        public DateTime? LockoutEndUtc { get => _lockoutEndUtc; set => SetProperty(ref _lockoutEndUtc, value); }

        private string _permissions = string.Empty;
        public string Permissions
        {
            get => _permissions;
            set
            {
                if (SetProperty(ref _permissions, value ?? string.Empty))
                    OnPropertyChanged(nameof(AccessSummary));
            }
        }

        public bool IsLockedOut => LockoutEndUtc.HasValue && LockoutEndUtc.Value > DateTime.UtcNow;

        public string LockoutStatus => IsLockedOut
            ? $"Locked until {LockoutEndUtc!.Value.ToLocalTime():g}"
            : FailedLoginAttempts > 0
                ? $"{FailedLoginAttempts} failed login attempt{(FailedLoginAttempts == 1 ? string.Empty : "s")}."
                : "Ready";

        public string AccessSummary
        {
            get
            {
                if (IsAdmin)
                    return "Full admin access";

                var labels = GetPermissionKeys()
                    .Select(key => PermissionLabels.TryGetValue(key, out var label) ? label : key)
                    .ToList();

                return labels.Count == 0
                    ? "No app sections assigned"
                    : string.Join(", ", labels);
            }
        }

        private MediaBrush _initialsBrush = MediaBrushes.Transparent;
        public MediaBrush InitialsBrush { get => _initialsBrush; set => SetProperty(ref _initialsBrush, value); }

        public bool HasPermission(string permissionKey)
        {
            if (IsAdmin)
                return true;
            if (string.IsNullOrWhiteSpace(permissionKey))
                return false;
            return GetPermissionKeys().Contains(permissionKey);
        }

        public bool HasAnyPermission(params string[] permissionKeys)
            => permissionKeys.Any(HasPermission);

        public void SetPermission(string permissionKey, bool allowed)
        {
            var permissions = GetPermissionKeys().ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (allowed)
                permissions.Add(permissionKey);
            else
                permissions.Remove(permissionKey);

            Permissions = permissions.Count == 0
                ? NoPermissionsValue
                : string.Join(";", PermissionLabels.Keys.Where(permissions.Contains));
        }

        IEnumerable<string> GetPermissionKeys()
        {
            if (string.Equals(Permissions, NoPermissionsValue, StringComparison.OrdinalIgnoreCase))
                return Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(Permissions))
                return DefaultUserPermissions;

            return Permissions
                .Split(new[] { ';', ',', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(key => PermissionLabels.ContainsKey(key))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public static string BuildPermissions(IEnumerable<string> permissionKeys)
        {
            var permissions = permissionKeys
                .Where(PermissionLabels.ContainsKey)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return permissions.Count == 0
                ? NoPermissionsValue
                : string.Join(";", PermissionLabels.Keys.Where(permissions.Contains));
        }
    }
}
