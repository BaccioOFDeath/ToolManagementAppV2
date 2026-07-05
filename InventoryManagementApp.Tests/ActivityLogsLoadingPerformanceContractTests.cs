using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ActivityLogsLoadingPerformanceContractTests
    {
        [Fact]
        public void ActivityLogsViewModel_DoesNotStartDuplicateConstructorLoad()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ActivityLogsViewModel.cs");

            Assert.Contains("RefreshCommand = new AsyncRelayCommand(LoadLogsAsync, () => CanRefreshActivityRows);", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsLoading", source, StringComparison.Ordinal);
            Assert.Contains("if (IsLoading)", source, StringComparison.Ordinal);
            Assert.Contains("return false;", source, StringComparison.Ordinal);
            Assert.Contains("IsLoading = true;", source, StringComparison.Ordinal);
            Assert.Contains("finally", source, StringComparison.Ordinal);
            Assert.Contains("IsLoading = false;", source, StringComparison.Ordinal);
            Assert.Contains("RefreshCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.DoesNotContain("_ = RefreshCommand.ExecuteAsync(null);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsPage_LoadsOncePerPageInstanceAndStillAllowsRefresh()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml.cs");

            Assert.Contains("private bool _hasLoadedLogs;", source, StringComparison.Ordinal);
            Assert.Contains("private int _loadVersion;", source, StringComparison.Ordinal);
            Assert.Contains("private ActivityLogsViewModel? _loadedViewModel;", source, StringComparison.Ordinal);
            Assert.Contains("if (DataContext is not ActivityLogsViewModel vm)", source, StringComparison.Ordinal);
            Assert.Contains("if (_hasLoadedLogs && ReferenceEquals(_loadedViewModel, vm))", source, StringComparison.Ordinal);
            Assert.Contains("if (!vm.RefreshCommand.CanExecute(null))", source, StringComparison.Ordinal);
            Assert.Contains("var loadVersion = _loadVersion;", source, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("if (!IsCurrentActivityLoad(vm, loadVersion) || !vm.RefreshCommand.CanExecute(null))", source, StringComparison.Ordinal);
            Assert.Contains("var loaded = await vm.LoadLogsAsync();", source, StringComparison.Ordinal);
            Assert.Contains("if (!IsCurrentActivityLoad(vm, loadVersion))", source, StringComparison.Ordinal);
            Assert.Contains("_hasLoadedLogs = loaded;", source, StringComparison.Ordinal);
            Assert.Contains("ActivityLogsPage_DataContextChanged", source, StringComparison.Ordinal);
            Assert.Contains("ActivityLogsPage_Unloaded", source, StringComparison.Ordinal);
            Assert.Contains("private bool IsCurrentActivityLoad(ActivityLogsViewModel vm, int loadVersion)", source, StringComparison.Ordinal);
            Assert.Contains("private async void RefreshLogs_Click", source, StringComparison.Ordinal);
            Assert.Contains("await vm.LoadLogsAsync();", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Loaded += async", source, StringComparison.Ordinal);
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
        static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");

    }
}
