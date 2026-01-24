using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace InventoryManagementApp.Utilities.Helpers
{
    public static class DatabaseSecurityHelper
    {
        public static string? GetPermissionWarning(string dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath) || !File.Exists(dbPath))
                return null;

            if (OperatingSystem.IsWindows())
                return GetWindowsPermissionWarning(dbPath);

            return GetUnixPermissionWarning(dbPath);
        }

        static string? GetWindowsPermissionWarning(string dbPath)
        {
            try
            {
                var fileInfo = new FileInfo(dbPath);
                var security = fileInfo.GetAccessControl();
                var rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

                foreach (FileSystemAccessRule rule in rules)
                {
                    if (rule.AccessControlType != AccessControlType.Allow)
                        continue;

                    if (!HasWriteAccess(rule.FileSystemRights))
                        continue;

                    if (rule.IdentityReference is SecurityIdentifier sid &&
                        (sid.IsWellKnown(WellKnownSidType.WorldSid) ||
                         sid.IsWellKnown(WellKnownSidType.BuiltinUsersSid)))
                    {
                        return "SQLite database file permissions allow write access to all users. Restrict the file to the application account or administrators only.";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Unable to verify database file permissions: {ex.Message}";
            }

            return null;
        }

        static bool HasWriteAccess(FileSystemRights rights)
        {
            const FileSystemRights writeRights =
                FileSystemRights.WriteData |
                FileSystemRights.AppendData |
                FileSystemRights.Modify |
                FileSystemRights.FullControl |
                FileSystemRights.Write |
                FileSystemRights.ChangePermissions |
                FileSystemRights.TakeOwnership;

            return (rights & writeRights) != 0;
        }

        static string? GetUnixPermissionWarning(string dbPath)
        {
            try
            {
                var mode = File.GetUnixFileMode(dbPath);
                var insecure = mode & (UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                                       UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute);

                if (insecure != 0)
                    return "SQLite database file permissions are too permissive. Restrict the file to owner-only access (e.g., chmod 600).";
            }
            catch (PlatformNotSupportedException)
            {
                return "SQLite database file permissions could not be verified on this platform.";
            }
            catch (Exception ex)
            {
                return $"Unable to verify database file permissions: {ex.Message}";
            }

            return null;
        }
    }
}
