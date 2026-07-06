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
            Assert.Contains("StatusTextBlock", xaml, StringComparison.Ordinal);
            Assert.Contains("{Binding StatusMessage}", xaml, StringComparison.Ordinal);
            Assert.Contains("{Binding FailureSummary}", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PasswordPromptWindow_UsesResponsiveScrollableScaledDesktopLayout()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PasswordPromptWindow.xaml");

            Assert.Contains("Width=\"600\" Height=\"420\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"520\" MinHeight=\"360\"", xaml, StringComparison.Ordinal);
            Assert.Contains("UseLayoutRounding=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SnapsToDevicePixels=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"260\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"150\" MinWidth=\"120\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"220\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Grid.Row=\"5\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PasswordPromptWindow_PreservesPasswordAndCommandWiring()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PasswordPromptWindow.xaml");
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PasswordPromptWindow.xaml.cs");

            Assert.Contains("x:Name=\"PromptTextBlock\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"PasswordBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("PasswordChanged=\"PasswordBox_PasswordChanged\"", xaml, StringComparison.Ordinal);
            Assert.Contains("PreviewKeyDown=\"PasswordBox_PreviewKeyDown\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ForgotPasswordButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ResetPasswordCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OkCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ResetRecoveryPanel.Visibility", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_attemptCount >= MaxAttempts", codeBehind, StringComparison.Ordinal);
            Assert.Contains("FocusPasswordBox", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Focus(PasswordBox)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DispatcherPriority.Input", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void PasswordPromptWindow_CoalescesFocusAndHandlesPasswordKeyboardFlow()
        {
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PasswordPromptWindow.xaml.cs");

            Assert.Contains("private DispatcherOperation? _pendingFocusOperation;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private bool _isUnloaded;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Unloaded += OnUnloaded;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private void AbortPendingFocus()", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_pendingFocusOperation.Abort();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("FocusPasswordBox(selectAll: true);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("PasswordBox.SelectAll();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private void PasswordBox_PreviewKeyDown", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Key == Key.Enter && VM.OkCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Key == Key.Escape && VM.CancelCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void PasswordPromptViewModel_GatesUnlockAndResetBusyState()
        {
            var viewModel = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "PasswordPromptViewModel.cs");

            Assert.Contains("private bool _isResetInProgress;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public string StatusMessage", viewModel, StringComparison.Ordinal);
            Assert.Contains("public string FailureSummary", viewModel, StringComparison.Ordinal);
            Assert.Contains("OkCommand = new RelayCommand(OnOk, CanSubmitPassword);", viewModel, StringComparison.Ordinal);
            Assert.Contains("ResetPasswordCommand = new AsyncRelayCommand(OnResetPasswordAsync, CanRequestReset);", viewModel, StringComparison.Ordinal);
            Assert.Contains("OkCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
            Assert.Contains("ResetPasswordCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
            Assert.Contains("!IsResetInProgress && !string.IsNullOrWhiteSpace(EnteredPassword)", viewModel, StringComparison.Ordinal);
            Assert.Contains("if (IsResetInProgress)", viewModel, StringComparison.Ordinal);
            Assert.Contains("try", viewModel, StringComparison.Ordinal);
            Assert.Contains("finally", viewModel, StringComparison.Ordinal);
            Assert.Contains("IsResetInProgress = false;", viewModel, StringComparison.Ordinal);
            Assert.Contains("RegisterFailedAttempt", viewModel, StringComparison.Ordinal);
            Assert.Contains("ClearPasswordFeedback", viewModel, StringComparison.Ordinal);
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
