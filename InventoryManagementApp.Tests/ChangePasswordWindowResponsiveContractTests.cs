using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ChangePasswordWindowResponsiveContractTests
    {
        [Fact]
        public void ChangePasswordWindow_UsesCompactBoundedResponsiveShell()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ChangePasswordWindow.xaml");

            Assert.Contains("Width=\"520\" Height=\"390\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"460\" MinHeight=\"340\"", xaml, StringComparison.Ordinal);
            Assert.Contains("UseLayoutRounding=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SnapsToDevicePixels=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Grid Margin=\"14\" ClipToBounds=\"True\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"560\" Height=\"410\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"520\" MinHeight=\"380\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ChangePasswordWindow_KeepsBodyScrollableAndFooterAnchored()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ChangePasswordWindow.xaml");

            Assert.Contains("<RowDefinition Height=\"*\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<controls:DialogButtonBar Grid.Row=\"2\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<controls:DialogButtonBar Grid.Row=\"3\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ChangePasswordWindow_BoundsHeaderHelpAndPasswordInputsForScaledDesktop()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ChangePasswordWindow.xaml");

            Assert.Contains("MaxHeight=\"118\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("TextTrimming=\"CharacterEllipsis\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxHeight=\"48\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"170\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"145\" MinWidth=\"108\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"180\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"420\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalAlignment=\"Stretch\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"170\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"260\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ChangePasswordWindow_ShowsReadinessAndValidationWithoutLayoutOverlap()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ChangePasswordWindow.xaml");

            Assert.Contains("Text=\"{Binding PasswordReadinessSummary}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxHeight=\"54\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding ValidationMessage}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"{Binding HasValidationMessage, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("RightButtonText=\"Save Password\"", xaml, StringComparison.Ordinal);
            Assert.Contains("RightCommand=\"{Binding SaveCommand}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ChangePasswordWindow_CodeBehindFocusesPasswordFieldAndCancelsStaleFocus()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ChangePasswordWindow.xaml.cs");

            Assert.Contains("using System.Windows.Threading;", source, StringComparison.Ordinal);
            Assert.Contains("DispatcherOperation? _pendingFocusOperation;", source, StringComparison.Ordinal);
            Assert.Contains("Loaded += ChangePasswordWindow_Loaded;", source, StringComparison.Ordinal);
            Assert.Contains("Activated += ChangePasswordWindow_Activated;", source, StringComparison.Ordinal);
            Assert.Contains("Unloaded += ChangePasswordWindow_Unloaded;", source, StringComparison.Ordinal);
            Assert.Contains("FocusNewPasswordBox(selectAll: true);", source, StringComparison.Ordinal);
            Assert.Contains("NewPasswordBox.Dispatcher.BeginInvoke", source, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Focus(NewPasswordBox);", source, StringComparison.Ordinal);
            Assert.Contains("NewPasswordBox.SelectAll();", source, StringComparison.Ordinal);
            Assert.Contains("AbortPendingFocus();", source, StringComparison.Ordinal);
            Assert.Contains("DispatcherOperationStatus.Pending", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ChangePasswordWindow_CodeBehindSupportsEnterEscapeAndDisposesCleanly()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ChangePasswordWindow.xaml");
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ChangePasswordWindow.xaml.cs");

            Assert.True(CountOccurrences(xaml, "PreviewKeyDown=\"PasswordBox_PreviewKeyDown\"") >= 2);
            Assert.Contains("void PasswordBox_PreviewKeyDown", source, StringComparison.Ordinal);
            Assert.Contains("e.Key == Key.Enter && VM.SaveCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("VM.SaveCommand.Execute(null);", source, StringComparison.Ordinal);
            Assert.Contains("e.Key == Key.Escape && VM.CancelCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("VM.CancelCommand.Execute(null);", source, StringComparison.Ordinal);
            Assert.Contains("AbortPendingFocus();\n            Close();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ChangePasswordViewModel_GatesSaveAndReportsReadiness()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ChangePasswordViewModel.cs");

            Assert.Contains("public bool HasValidationMessage => !string.IsNullOrWhiteSpace(ValidationMessage);", source, StringComparison.Ordinal);
            Assert.Contains("public bool CanAttemptSave =>", source, StringComparison.Ordinal);
            Assert.Contains("!string.IsNullOrWhiteSpace(NewPassword) && !string.IsNullOrWhiteSpace(ConfirmPassword)", source, StringComparison.Ordinal);
            Assert.Contains("public string PasswordReadinessSummary", source, StringComparison.Ordinal);
            Assert.Contains("Enter and confirm the new password to enable Save Password.", source, StringComparison.Ordinal);
            Assert.Contains("Ready to validate and save the new password.", source, StringComparison.Ordinal);
            Assert.Contains("SaveCommand = new RelayCommand(() =>", source, StringComparison.Ordinal);
            Assert.Contains("}, () => CanAttemptSave);", source, StringComparison.Ordinal);
            Assert.Contains("SaveCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(PasswordReadinessSummary));", source, StringComparison.Ordinal);
        }

        private static int CountOccurrences(string text, string value)
        {
            var count = 0;
            var startIndex = 0;
            while (true)
            {
                var index = text.IndexOf(value, startIndex, StringComparison.Ordinal);
                if (index < 0)
                    return count;

                count++;
                startIndex = index + value.Length;
            }
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }
}