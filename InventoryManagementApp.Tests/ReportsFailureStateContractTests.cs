using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReportsFailureStateContractTests
    {
        [Fact]
        public void ReportGenerationFailureClearsLastRunTimestamp()
        {
            var source = File.ReadAllText(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "InventoryManagementApp",
                "ViewModels",
                "ReportsViewModel.cs"));

            var catchStart = source.IndexOf("catch (Exception ex)", StringComparison.Ordinal);
            Assert.True(catchStart >= 0, "ReportsViewModel must keep an explicit report-generation failure branch.");

            var catchEnd = source.IndexOf("finally", catchStart, StringComparison.Ordinal);
            Assert.True(catchEnd > catchStart, "The report-generation failure branch should stay before the finally block.");

            var failureBody = source.Substring(catchStart, catchEnd - catchStart);

            Assert.Contains("ReportLines.Clear();", failureBody);
            Assert.Contains("SelectedReportLine = null;", failureBody);
            Assert.Contains("ReportStatus = \"Report failed.\";", failureBody);
            Assert.Contains("LastRunAt = null;", failureBody);
            Assert.DoesNotContain("LastRunAt = DateTime.Now;", failureBody);
            Assert.Contains("OnPropertyChanged(nameof(LastRunText));", source);
            Assert.Contains("LastRunAt.HasValue", source);
        }
    }
}
