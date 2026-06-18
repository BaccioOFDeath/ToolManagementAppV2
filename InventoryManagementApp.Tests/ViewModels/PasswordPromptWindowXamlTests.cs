using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class PasswordPromptWindowXamlTests
    {
        [Fact]
        public void PasswordPromptWindow_UsesPolishedResetRecoveryPanelAndFooter()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PasswordPromptWindow.xaml");

            Assert.Contains("Secure Access", xaml, StringComparison.Ordinal);
            Assert.Contains("Password Reset Request", xaml, StringComparison.Ordinal);
            Assert.Contains("ResetRecoveryPanel", xaml, StringComparison.Ordinal);
            Assert.Contains("Request Reset", xaml, StringComparison.Ordinal);
            Assert.Contains("Auth footer status", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PasswordPromptWindow_PreservesPasswordAndCommandWiring()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PasswordPromptWindow.xaml");
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PasswordPromptWindow.xaml.cs");

            Assert.Contains("x:Name=\"PromptTextBlock\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"PasswordBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("PasswordChanged=\"PasswordBox_PasswordChanged\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ForgotPasswordButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ResetPasswordCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OkCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ResetRecoveryPanel.Visibility", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_attemptCount >= MaxAttempts", codeBehind, StringComparison.Ordinal);
        }

        static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                directory = directory.Parent;

            Assert.NotNull(directory);
            var path = Path.Combine(directory!.FullName, Path.Combine(relativePathParts));
            Assert.True(File.Exists(path), $"Expected repository file at {path}");
            return File.ReadAllText(path);
        }
    }
}
