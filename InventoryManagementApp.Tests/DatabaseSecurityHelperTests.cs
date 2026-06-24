using InventoryManagementApp.Utilities.Helpers;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class DatabaseSecurityHelperTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("non-existent-file.db")]
        public void GetPermissionWarning_WhenFileMissing_ReturnsNull(string path)
        {
            var warning = DatabaseSecurityHelper.GetPermissionWarning(path);

            Assert.Null(warning);
        }

        [Fact]
        public void EnsureDatabaseFileSecurity_WhenFileMissing_CreatesDatabaseFile()
        {
            var dir = Path.Combine(Path.GetTempPath(), "InventoryManagementAppTests", Guid.NewGuid().ToString("N"));
            var dbPath = Path.Combine(dir, "inventory.db");

            try
            {
                var warning = DatabaseSecurityHelper.EnsureDatabaseFileSecurity(dbPath);

                Assert.True(File.Exists(dbPath));
                Assert.Null(warning);
            }
            finally
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
        }

        [Fact]
        public void EnsureDatabaseFileSecurity_WhenSidecarsExist_SecuresSidecars()
        {
            var dir = Path.Combine(Path.GetTempPath(), "InventoryManagementAppTests", Guid.NewGuid().ToString("N"));
            var dbPath = Path.Combine(dir, "inventory.db");
            var walPath = dbPath + "-wal";
            var shmPath = dbPath + "-shm";

            try
            {
                Directory.CreateDirectory(dir);
                File.WriteAllText(dbPath, "");
                File.WriteAllText(walPath, "");
                File.WriteAllText(shmPath, "");

                var warning = DatabaseSecurityHelper.EnsureDatabaseFileSecurity(dbPath);

                Assert.Null(warning);
                Assert.True(File.Exists(walPath));
                Assert.True(File.Exists(shmPath));
            }
            finally
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
        }

        [Fact]
        public void EnsureDatabaseFileSecurity_WhenWindowsFileAllowsUsersWrite_RemovesBroadWriteAccess()
        {
            if (!OperatingSystem.IsWindows())
                return;

            var dir = Path.Combine(Path.GetTempPath(), "InventoryManagementAppTests", Guid.NewGuid().ToString("N"));
            var dbPath = Path.Combine(dir, "inventory.db");

            try
            {
                Directory.CreateDirectory(dir);
                File.WriteAllText(dbPath, "");

                var fileInfo = new FileInfo(dbPath);
                var security = fileInfo.GetAccessControl();
                security.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                    FileSystemRights.Modify,
                    AccessControlType.Allow));
                fileInfo.SetAccessControl(security);

                Assert.NotNull(DatabaseSecurityHelper.GetPermissionWarning(dbPath));

                var warning = DatabaseSecurityHelper.EnsureDatabaseFileSecurity(dbPath);

                Assert.Null(warning);
                Assert.Null(DatabaseSecurityHelper.GetPermissionWarning(dbPath));
            }
            finally
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
        }
    }
}
