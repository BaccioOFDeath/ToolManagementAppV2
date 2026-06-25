using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;

namespace InventoryManagementApp.Utilities.Helpers
{
    public static class DatabaseSecurityHelper
    {
        public static string? EnsureDatabaseFileSecurity(string dbPath, bool securePermissions = true)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
                return null;

            try
            {
                var directory = Path.GetDirectoryName(dbPath);
                var createdDirectory = false;
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    createdDirectory = true;
                }

                if (!File.Exists(dbPath))
                    using (File.Create(dbPath)) { }

                if (!securePermissions)
                    return null;

                if (OperatingSystem.IsWindows())
                {
                    if (createdDirectory && !string.IsNullOrWhiteSpace(directory))
                        SecureWindowsDirectory(directory);

                    SecureWindowsFile(dbPath);
                    SecureWindowsSidecarFile(dbPath + "-wal");
                    SecureWindowsSidecarFile(dbPath + "-shm");
                }
                else
                {
                    if (createdDirectory && !string.IsNullOrWhiteSpace(directory))
                        SecureUnixDirectory(directory);

                    SecureUnixFile(dbPath);
                    SecureUnixSidecarFile(dbPath + "-wal");
                    SecureUnixSidecarFile(dbPath + "-shm");
                }
            }
            catch (Exception ex)
            {
                return $"Unable to secure database file permissions: {ex.Message}";
            }

            return GetPermissionWarning(dbPath);
        }

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

        static void SecureWindowsSidecarFile(string path)
        {
            if (File.Exists(path))
                SecureWindowsFile(path);
        }

        static void SecureWindowsFile(string path)
        {
            var fileInfo = new FileInfo(path);
            var security = fileInfo.GetAccessControl();
            ProtectWindowsSecurity(security);
            fileInfo.SetAccessControl(security);
        }

        static void SecureWindowsDirectory(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            var security = directoryInfo.GetAccessControl();
            ProtectWindowsSecurity(security);
            directoryInfo.SetAccessControl(security);
        }

        static void ProtectWindowsSecurity(FileSystemSecurity security)
        {
            security.SetAccessRuleProtection(isProtected: true, preserveInheritance: true);
            RemoveBroadWriteAccess(security, WellKnownSidType.WorldSid);
            RemoveBroadWriteAccess(security, WellKnownSidType.BuiltinUsersSid);

            var currentUser = WindowsIdentity.GetCurrent().User;
            if (currentUser != null)
                security.AddAccessRule(new FileSystemAccessRule(currentUser, FileSystemRights.Modify, AccessControlType.Allow));

            security.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                FileSystemRights.FullControl,
                AccessControlType.Allow));
            security.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                FileSystemRights.FullControl,
                AccessControlType.Allow));
        }

        static void RemoveBroadWriteAccess(FileSystemSecurity security, WellKnownSidType sidType)
        {
            var sid = new SecurityIdentifier(sidType, null);
            var rules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.AccessControlType != AccessControlType.Allow ||
                    rule.IdentityReference is not SecurityIdentifier ruleSid ||
                    !ruleSid.Equals(sid) ||
                    !HasWriteAccess(rule.FileSystemRights))
                {
                    continue;
                }

                security.RemoveAccessRuleSpecific(rule);
            }
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

        [UnsupportedOSPlatform("windows")]
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

        [UnsupportedOSPlatform("windows")]
        static void SecureUnixDirectory(string path)
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        [UnsupportedOSPlatform("windows")]
        static void SecureUnixSidecarFile(string path)
        {
            if (File.Exists(path))
                SecureUnixFile(path);
        }

        [UnsupportedOSPlatform("windows")]
        static void SecureUnixFile(string path)
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }
}
