using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReportsPrintDocumentContractTests
    {
        [Fact]
        public void ReportsPrintDocumentUsesFlexibleProfessionalHandoffLayout()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml.cs");

            Assert.Contains("PagePadding = new Thickness(36)", source, StringComparison.Ordinal);
            Assert.Contains("ColumnGap = 0", source, StringComparison.Ordinal);
            Assert.Contains("BuildSummarySection(safeTitle, summary, lastRunText, safeLines.Count)", source, StringComparison.Ordinal);
            Assert.Contains("AddKeyValueRow(group, \"Report\", title)", source, StringComparison.Ordinal);
            Assert.Contains("AddKeyValueRow(group, \"Action Rows\", lineCount.ToString())", source, StringComparison.Ordinal);
            Assert.Contains("AddKeyValueRow(group, \"Last Run\", ValueOrNotRecorded(lastRunText))", source, StringComparison.Ordinal);
            Assert.Contains("AddKeyValueRow(group, \"Summary\", ValueOrNotRecorded(summary))", source, StringComparison.Ordinal);
            Assert.Contains("Tag = \"KeyValue\"", source, StringComparison.Ordinal);
            Assert.Contains("table.Columns.Add(new TableColumn { Width = new GridLength(0.08, GridUnitType.Star) });", source, StringComparison.Ordinal);
            Assert.Contains("table.Columns.Add(new TableColumn { Width = new GridLength(0.36, GridUnitType.Star) });", source, StringComparison.Ordinal);
            Assert.Contains("AddCell(header, \"Entry\", true)", source, StringComparison.Ordinal);
            Assert.Contains("AddCell(header, \"Report Detail\", true)", source, StringComparison.Ordinal);
            Assert.Contains("AddCell(header, \"Next Action\", true)", source, StringComparison.Ordinal);
            Assert.Contains("No report rows were available when this packet was prepared.", source, StringComparison.Ordinal);
            Assert.Contains("Review each destination, source-page route, and next action", source, StringComparison.Ordinal);
            Assert.Contains("ValueOrNotRecorded(line.Category)", source, StringComparison.Ordinal);
            Assert.Contains("ValueOrNotRecorded(line.DestinationName)", source, StringComparison.Ordinal);
            Assert.Contains("ValueOrNotRecorded(line.Text)", source, StringComparison.Ordinal);
            Assert.Contains("ValueOrNotRecorded(line.ActionHint)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("foreach (var width in new[] { 45.0, 85.0, 105.0, 300.0, 205.0 })", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new GridLength(300.0)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new GridLength(205.0)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportsPrintDocumentPreservesPreviewRouteAndReportActions()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml.cs");
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml");

            Assert.Contains("new PrintPreviewWindow().ShowPreview(", source, StringComparison.Ordinal);
            Assert.Contains("Review the report summary, destination routing, and next-action handoff before printing.", source, StringComparison.Ordinal);
            Assert.Contains("OpenSourcePage_Click", source, StringComparison.Ordinal);
            Assert.Contains("CopySelectedRow_Click", source, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e)", source, StringComparison.Ordinal);
            Assert.Contains("Print Report", xaml, StringComparison.Ordinal);
            Assert.Contains("Copy Handoff", xaml, StringComparison.Ordinal);
            Assert.Contains("Open Source Page", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
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
