using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UsersEditWindowResponsiveContractTests
    {
        [Fact]
        public void UsersEditWindow_UsesCompactBoundedResponsiveShell()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "UsersEditWindow.xaml");
            var code = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "UsersEditWindow.xaml.cs");

            Assert.Contains("Width=\"900\" Height=\"680\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"640\" MinHeight=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("UseLayoutRounding=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SnapsToDevicePixels=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Grid Margin=\"12\" ClipToBounds=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("this.UseResponsiveDefaultSize(920, 700);", code, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"1080\" Height=\"820\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"860\" MinHeight=\"680\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("this.UseResponsiveDefaultSize(1120, 880);", code, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersEditWindow_WrapsHeaderActionsAndStepCards()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "UsersEditWindow.xaml");

            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxHeight=\"126\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Column=\"1\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Right\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"260\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Style x:Key=\"UserEditorStepCard\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,2\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"178\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"238\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Row=\"1\" Columns=\"4\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersEditWindow_LowersEditorSplitPressureAndKeepsBodyScrollable()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "UsersEditWindow.xaml");

            Assert.Contains("<ScrollViewer VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding CanEditProfile}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"160\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.02*\" MinWidth=\"250\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.48*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Column=\"1\" Width=\"8\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Grid MinHeight=\"520\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"190\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"*\" MinWidth=\"610\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersEditWindow_BoundsProfileFieldsAndPermissionChecklist()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "UsersEditWindow.xaml");

            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource AdminHandoffCard}\" Margin=\"0\" MaxWidth=\"180\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Width=\"112\" Height=\"112\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"92\" MinWidth=\"78\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"120\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"62\" MaxHeight=\"96\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"2\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\" CanContentScroll=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.05*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"96\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Height=\"72\" TextWrapping=\"Wrap\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"12\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersEditWindow_ShowsSaveReadinessAndSavingOverlay()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "UsersEditWindow.xaml");

            Assert.Contains("Text=\"{Binding SaveStatusText}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"{Binding IsSaving, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"Saving user profile\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding CanEditProfile}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<controls:SaveCancelBar Grid.Row=\"3\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Text=\"User profile ready\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void UsersEditViewModel_GatesSaveAndEditorCommandsDuringSave()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "UsersEditViewModel.cs");

            Assert.Contains("private bool _isSaving;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsSaving", source, StringComparison.Ordinal);
            Assert.Contains("public bool CanEditProfile => !IsSaving;", source, StringComparison.Ordinal);
            Assert.Contains("public bool CanSaveUserProfile =>", source, StringComparison.Ordinal);
            Assert.Contains("!string.IsNullOrWhiteSpace(EditingUser.UserName)", source, StringComparison.Ordinal);
            Assert.Contains("public string SaveStatusText", source, StringComparison.Ordinal);
            Assert.Contains("SaveCommand = new AsyncRelayCommand(SaveAsync, () => CanSaveUserProfile);", source, StringComparison.Ordinal);
            Assert.Contains("CancelCommand = new RelayCommand(onCancel, () => CanEditProfile);", source, StringComparison.Ordinal);
            Assert.Contains("BrowseImageCommand = new RelayCommand(BrowseImage, () => CanEditProfile);", source, StringComparison.Ordinal);
            Assert.Contains("SelectAdvisorPresetCommand = new RelayCommand", source, StringComparison.Ordinal);
            Assert.Contains("NotifyEditorCommandStatesChanged();", source, StringComparison.Ordinal);
            Assert.Contains("async Task SaveAsync()", source, StringComparison.Ordinal);
            Assert.Contains("IsSaving = true;", source, StringComparison.Ordinal);
            Assert.Contains("await _onSave();", source, StringComparison.Ordinal);
            Assert.Contains("IsSaving = false;", source, StringComparison.Ordinal);
        }

        [Fact]
        public void SaveCancelBar_UsesResponsiveWrappingActions()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Controls", "SaveCancelBar.xaml");

            Assert.Contains("ClipToBounds=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Grid MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("TextTrimming=\"CharacterEllipsis\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxHeight=\"42\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Column=\"1\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"96\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel Grid.Column=\"1\" Orientation=\"Horizontal\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"104\"", xaml, StringComparison.Ordinal);
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