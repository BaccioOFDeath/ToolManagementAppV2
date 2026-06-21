using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MainViewModelSwitchUserContractTests
    {
        [Fact]
        public void SwitchUserSuccessKeepsShellOnAuthenticatedDashboard()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MainViewModel.cs");
            var switchUserStart = source.IndexOf("SwitchUserCommand = new AsyncRelayCommand", StringComparison.Ordinal);
            var exitCommandStart = source.IndexOf("ExitCommand = new RelayCommand", StringComparison.Ordinal);

            Assert.True(switchUserStart >= 0, "SwitchUserCommand source block was not found.");
            Assert.True(exitCommandStart > switchUserStart, "SwitchUserCommand block should appear before ExitCommand.");

            var switchUserSource = source.Substring(switchUserStart, exitCommandStart - switchUserStart);
            var loginSuccessIndex = switchUserSource.IndexOf("if (await _showLoginWindow())", StringComparison.Ordinal);
            var loginCancelledIndex = switchUserSource.IndexOf("else", loginSuccessIndex, StringComparison.Ordinal);

            Assert.True(loginSuccessIndex >= 0, "SwitchUserCommand should branch on a successful login result.");
            Assert.True(loginCancelledIndex > loginSuccessIndex, "SwitchUserCommand should keep a separate cancelled-login branch.");

            var signOutPreparation = switchUserSource.Substring(0, loginSuccessIndex);
            Assert.Contains("_userContext.CurrentUser = null", signOutPreparation, StringComparison.Ordinal);
            Assert.Contains("RefreshCurrentUser();", signOutPreparation, StringComparison.Ordinal);
            Assert.Contains("CloseNonMainWindows();", signOutPreparation, StringComparison.Ordinal);
            Assert.Contains("ClearSearch();", signOutPreparation, StringComparison.Ordinal);
            Assert.Contains("SetNavSection(NavSectionKeys.Overview);", signOutPreparation, StringComparison.Ordinal);
            Assert.Contains("await OpenDashboardCommand.ExecuteAsync(null);", signOutPreparation, StringComparison.Ordinal);

            var loginSuccessBranch = switchUserSource.Substring(loginSuccessIndex, loginCancelledIndex - loginSuccessIndex);
            Assert.Contains("await OpenDashboardCommand.ExecuteAsync(null);", loginSuccessBranch, StringComparison.Ordinal);
            Assert.DoesNotContain("_shutdownApplication", loginSuccessBranch, StringComparison.Ordinal);

            var loginCancelledBranch = switchUserSource.Substring(loginCancelledIndex);
            Assert.Contains("_shutdownApplication();", loginCancelledBranch, StringComparison.Ordinal);
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}
