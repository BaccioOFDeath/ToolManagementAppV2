using System.IO;
using InventoryManagementApp.Utilities;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class DatabasePathResolverTests
    {
        [Fact]
        public void Resolve_RelativePath_UsesExecutableDirectory()
        {
            var baseDirectory = Path.Combine(Path.GetTempPath(), "InventoryManagementAppTests");

            var resolved = DatabasePathResolver.Resolve("Data/inventory.db", baseDirectory);

            Assert.Equal(Path.GetFullPath(Path.Combine(baseDirectory, "Data/inventory.db")), resolved);
        }

        [Fact]
        public void Resolve_RelativePathInsideSideBySideRelease_UsesDeploymentRoot()
        {
            var deploymentRoot = Path.Combine(Path.GetTempPath(), "InventoryManagementAppTests", "V2");
            var releaseDirectory = Path.Combine(deploymentRoot, "_releases", "2026.06.26-170000");

            var resolved = DatabasePathResolver.Resolve("Assets/Data/inventory.db", releaseDirectory);

            Assert.Equal(Path.GetFullPath(Path.Combine(deploymentRoot, "Assets/Data/inventory.db")), resolved);
        }

        [Fact]
        public void Resolve_RelativePathWithDeploymentRootEnvironment_UsesSharedDeploymentRoot()
        {
            var deploymentRoot = Path.Combine(Path.GetTempPath(), "InventoryManagementAppTests", "SharedV2");
            var localCacheDirectory = Path.Combine(Path.GetTempPath(), "InventoryManagementAppTests", "LocalCache");
            var original = System.Environment.GetEnvironmentVariable(DeploymentPathResolver.DeploymentRootEnvironmentVariable);

            try
            {
                System.Environment.SetEnvironmentVariable(DeploymentPathResolver.DeploymentRootEnvironmentVariable, deploymentRoot);

                var resolved = DatabasePathResolver.Resolve("Assets/Data/inventory.db", localCacheDirectory);

                Assert.Equal(Path.GetFullPath(Path.Combine(deploymentRoot, "Assets/Data/inventory.db")), resolved);
            }
            finally
            {
                System.Environment.SetEnvironmentVariable(DeploymentPathResolver.DeploymentRootEnvironmentVariable, original);
            }
        }

        [Fact]
        public void Resolve_AbsolutePath_KeepsConfiguredPath()
        {
            var configured = Path.Combine(Path.GetTempPath(), "inventory.db");

            var resolved = DatabasePathResolver.Resolve(configured, @"C:\App");

            Assert.Equal(Path.GetFullPath(configured), resolved);
        }

        [Fact]
        public void IsSharedPath_UncPath_ReturnsTrue()
        {
            Assert.True(DatabasePathResolver.IsSharedPath(@"\\server\share\InventoryManagementApp\Data\inventory.db"));
        }
    }
}
