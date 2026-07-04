using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ActivityLogsFailureStateContractTests
    {
        [Fact]
        public void LoadFailureClearsRowsSelectionAndLastLoadedTimestamp()
        {
            var source = File.ReadAllText(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "InventoryManagementApp",
                "ViewModels",
                "ActivityLogsViewModel.cs"));

            var helperStart = source.IndexOf("private void PreserveActivityLogRowsAfterLoadFailure", StringComparison.Ordinal);
            Assert.True(helperStart >= 0, "ActivityLogsViewModel must keep a single helper for load-failure recovery.");

            var helperEnd = source.IndexOf("private void ClearFilters", helperStart, StringComparison.Ordinal);
            Assert.True(helperEnd > helperStart, "The load-failure cleanup helper should stay before ClearFilters.");

            var helperBody = source.Substring(helperStart, helperEnd - helperStart);

            Assert.Contains("if (Logs.Count == 0)", helperBody);
            Assert.Contains("FilteredLogs.Clear();", helperBody);
            Assert.Contains("SelectedLog = null;", helperBody);
            Assert.Contains("LastLoadedAt = null;", helperBody);
            Assert.Contains("RebuildFilterLists();", helperBody);
            Assert.Contains("NotifyActivityStateChanged();", helperBody);
            Assert.Contains("StatusMessage = message;", helperBody);
            Assert.Contains("OnPropertyChanged(nameof(LastLoadedText));", source);
        }
    }
}
