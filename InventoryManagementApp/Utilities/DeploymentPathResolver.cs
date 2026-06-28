using System;
using System.IO;

namespace InventoryManagementApp.Utilities
{
    public static class DeploymentPathResolver
    {
        public const string DeploymentRootEnvironmentVariable = "INVENTORYMANAGEMENTAPP_DEPLOYMENT_ROOT";

        public static string GetDeploymentRoot(string baseDirectory)
        {
            var configuredDeploymentRoot = Environment.GetEnvironmentVariable(DeploymentRootEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(configuredDeploymentRoot))
            {
                return Path.GetFullPath(configuredDeploymentRoot.Trim());
            }

            var fullBaseDirectory = Path.GetFullPath(baseDirectory);
            var releaseDirectory = new DirectoryInfo(fullBaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (releaseDirectory.Parent?.Name.Equals("_releases", StringComparison.OrdinalIgnoreCase) == true &&
                releaseDirectory.Parent.Parent is { } deploymentRoot)
            {
                return Path.GetFullPath(deploymentRoot.FullName);
            }

            return fullBaseDirectory;
        }

        public static string Resolve(string? configuredPath, string baseDirectory, string defaultRelativePath)
        {
            var path = string.IsNullOrWhiteSpace(configuredPath)
                ? defaultRelativePath
                : Environment.ExpandEnvironmentVariables(configuredPath.Trim());

            return Path.GetFullPath(Path.IsPathFullyQualified(path)
                ? path
                : Path.Combine(GetDeploymentRoot(baseDirectory), path));
        }
    }
}
