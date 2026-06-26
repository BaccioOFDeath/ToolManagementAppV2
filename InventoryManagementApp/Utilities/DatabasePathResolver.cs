using System;
using System.IO;

namespace InventoryManagementApp.Utilities
{
    public static class DatabasePathResolver
    {
        public static string Resolve(string? configuredPath, string baseDirectory)
        {
            return DeploymentPathResolver.Resolve(configuredPath, baseDirectory, "inventory.db");
        }

        public static bool IsSharedPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var fullPath = Path.GetFullPath(path);
            if (fullPath.StartsWith(@"\\", StringComparison.Ordinal))
                return true;

            try
            {
                var root = Path.GetPathRoot(fullPath);
                return !string.IsNullOrWhiteSpace(root) &&
                       new DriveInfo(root).DriveType == DriveType.Network;
            }
            catch
            {
                return false;
            }
        }
    }
}
