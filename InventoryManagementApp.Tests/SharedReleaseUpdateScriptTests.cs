using System;
using System.IO;
using System.Linq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class SharedReleaseUpdateScriptTests
    {
        [Fact]
        public void UpdateScriptSupportsSideBySideActiveUserDeployment()
        {
            var script = ReadRepositoryFile("scripts", "update-shared-release.ps1");

            Assert.Contains("[ValidateSet(\"InPlace\", \"SideBySide\")]", script, StringComparison.Ordinal);
            Assert.Contains("$DeploymentMode = \"InPlace\"", script, StringComparison.Ordinal);
            Assert.Contains("$releaseRoot = Join-Path $destinationPath \"_releases\"", script, StringComparison.Ordinal);
            Assert.Contains("$currentReleaseMarker = Join-Path $destinationPath \"current-release.txt\"", script, StringComparison.Ordinal);
            Assert.Contains("Set-Content -LiteralPath $currentReleaseMarker -Value $ReleaseName", script, StringComparison.Ordinal);
            Assert.Contains("rerun with -DeploymentMode SideBySide", script, StringComparison.Ordinal);
        }

        [Fact]
        public void DeploymentGuideDocumentsActiveUserUpdateFlowAndMigrationLimitation()
        {
            var guide = ReadRepositoryFile("SERVER_DEPLOYMENT_GUIDE.md");

            Assert.Contains("## Updating While Users Are Active", guide, StringComparison.Ordinal);
            Assert.Contains("-DeploymentMode SideBySide", guide, StringComparison.Ordinal);
            Assert.Contains("current-release.txt", guide, StringComparison.Ordinal);
            Assert.Contains("database migration", guide, StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var repositoryRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", ".."));
            return File.ReadAllText(Path.Combine(new[] { repositoryRoot }.Concat(relativePathParts).ToArray()));
        }
    }
}
