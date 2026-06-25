using System;
using System.IO;
using System.Reflection;
using InventoryManagementApp.Utilities.Helpers;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class PathHelperTests
    {
        [Fact]
        public void AssetBaseDirectory_UsesDeploymentRootForSideBySideRelease()
        {
            var method = typeof(PathHelper).GetMethod("GetAssetBaseDirectory", BindingFlags.NonPublic | BindingFlags.Static)!;
            var releaseBase = Path.Combine("X:\\V2", "_releases", "2026.06.26-100233") + Path.DirectorySeparatorChar;

            var assetBase = Assert.IsType<string>(method.Invoke(null, new object[] { releaseBase }));

            Assert.Equal(Path.GetFullPath("X:\\V2"), assetBase.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        [Fact]
        public void PathHelperAllowsSharedAssetRootAndReleaseRoot()
        {
            var method = typeof(PathHelper).GetMethod("IsWithinDirectory", BindingFlags.NonPublic | BindingFlags.Static)!;

            Assert.True(Invoke(method, "X:\\V2\\Assets\\CompanyLogo\\SDLogo.png", "X:\\V2"));
            Assert.True(Invoke(method, "X:\\V2\\_releases\\2026.06.26-100233\\Resources\\DefaultLogo.png", "X:\\V2\\_releases\\2026.06.26-100233"));
            Assert.False(Invoke(method, "X:\\Other\\Assets\\CompanyLogo\\SDLogo.png", "X:\\V2"));
        }

        static bool Invoke(MethodInfo method, string fullPath, string directory)
            => Assert.IsType<bool>(method.Invoke(null, new object[] { fullPath, directory }));
    }
}
