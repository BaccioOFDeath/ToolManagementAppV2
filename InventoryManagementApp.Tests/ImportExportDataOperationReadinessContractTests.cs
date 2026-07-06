using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ImportExportDataOperationReadinessContractTests
    {
        [Fact]
        public void ImportExportViewModel_WrapsImageMappingInSharedReadinessCommand()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("private readonly IAsyncRelayCommand _openImageImportMappingWindowCommand;", viewModel, StringComparison.Ordinal);
            Assert.Contains("_openImageImportMappingWindowCommand = openImageImportMappingWindowCommand ?? new AsyncRelayCommand(ct => Task.CompletedTask);", viewModel, StringComparison.Ordinal);
            Assert.Contains("OpenImageImportMappingWindowCommand = new AsyncRelayCommand(ct => OpenImageImportMappingAsync(ct), () => CanOpenImageImportMapping);", viewModel, StringComparison.Ordinal);
            Assert.Contains("async Task OpenImageImportMappingAsync(CancellationToken cancellationToken)", viewModel, StringComparison.Ordinal);
            Assert.Contains("if (!CanOpenImageImportMapping)", viewModel, StringComparison.Ordinal);
            Assert.Contains("await _openImageImportMappingWindowCommand.ExecuteAsync(null);", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_ExposesImageMappingReadinessWithPermissionAndBusyState()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("public bool CanOpenImageImportMapping => CanImportImages && !IsDataOperationBusy;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public string ActiveDataOperationName => ValueOrDefault(_currentDataOperation, \"Data operation\");", viewModel, StringComparison.Ordinal);
            Assert.Contains("Image mapping is paused while {ActiveDataOperationName.ToLowerInvariant()} is running", viewModel, StringComparison.Ordinal);
            Assert.Contains("Image import requires the {User.PermissionLabels[User.PermissionImportExport]} permission", viewModel, StringComparison.Ordinal);
            Assert.Contains("Image import is available for matching photos", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_ReportsBusyBackupAndRestoreStateInSharedSummary()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("public string BackupSummary => IsDataOperationBusy", viewModel, StringComparison.Ordinal);
            Assert.Contains("Backup and restore are paused while {ActiveDataOperationName.ToLowerInvariant()} is running.", viewModel, StringComparison.Ordinal);
            Assert.Contains("Finish or cancel the current data operation before starting another import, export, backup, restore, image mapping, copy, or print handoff.", viewModel, StringComparison.Ordinal);
            Assert.Contains("Ready for the next import, export, backup, restore, image mapping, copy, or print handoff.", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_NotifiesImageMappingReadinessOnUserAndBusyTransitions()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("OnPropertyChanged(nameof(CanOpenImageImportMapping));", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(ActiveDataOperationName));", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(ImageImportSummary));", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(BackupSummary));", viewModel, StringComparison.Ordinal);
            Assert.Contains("OpenImageImportMappingWindowCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_UsesWrappedImageMappingCommandEverywhere()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.True(CountOccurrences(xaml, "Command=\"{Binding OpenImageImportMappingWindowCommand}\"") >= 2);
            Assert.Contains("Visibility=\"{Binding IsCurrentUserAdmin, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Click=\"OpenImageImportMapping", xaml, StringComparison.Ordinal);
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
