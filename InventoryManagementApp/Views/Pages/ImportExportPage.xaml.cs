using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Windows;
using WpfMessageBox = System.Windows.MessageBox;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ImportExportPage : Page
    {
        private const int MaxPrintedLogRows = 250;

        public ImportExportPage()
        {
            InitializeComponent();
        }

        private void ImportExportLogGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedLog_Click(sender, e);
        }

        private void ImportExportLogRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            GridContextMenuSelection.SelectRow(sender, e);
        }

        private void OpenSelectedLog_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Import / Export", () =>
            {
                if (IsDataOperationBusy())
                {
                    WpfMessageBox.Show("Wait for the current import, export, backup, or restore operation to finish before opening log details.", "Import / Export", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var log = GetSelectedLogForAction();
                if (string.IsNullOrWhiteSpace(log))
                {
                    WpfMessageBox.Show("Select an import/export log row first.", "Import / Export", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                DetailDialogWindow.ShowDialogFor(
                    Window.GetWindow(this),
                    "Import / Export Result",
                    "Import / Export Result",
                    log,
                    "Review the selected operation result before copying, printing, or continuing the data workflow.",
                    "Run Log",
                    "Close returns to the run log with the selected result still available for copy, print, or review.");
            });
        }

        private void CopySelectedLog_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Import / Export", () =>
            {
                if (IsDataOperationBusy())
                {
                    WpfMessageBox.Show("Wait for the current import, export, backup, or restore operation to finish before copying log details.", "Import / Export", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var log = GetSelectedLogForAction();
                if (string.IsNullOrWhiteSpace(log))
                {
                    WpfMessageBox.Show("Select an import/export log row first.", "Import / Export", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                System.Windows.Clipboard.SetText(log);
            });
        }

        private string GetSelectedLogForAction()
        {
            if (ImportExportLogGrid.SelectedItem is string gridLog && !string.IsNullOrWhiteSpace(gridLog))
                return gridLog;

            return DataContext is ImportExportViewModel vm && !string.IsNullOrWhiteSpace(vm.SelectedImportExportLog)
                ? vm.SelectedImportExportLog
                : string.Empty;
        }

        private bool IsDataOperationBusy() =>
            DataContext is ImportExportViewModel vm && vm.IsDataOperationBusy;

        private void PrintLogs_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Import / Export", () =>
            {
                if (IsDataOperationBusy())
                {
                    WpfMessageBox.Show("Wait for the current import, export, backup, or restore operation to finish before generating a print preview.", "Import / Export", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var selectedLog = GetSelectedLogForAction();
                if (!string.IsNullOrWhiteSpace(selectedLog))
                {
                    var selectedDocument = BuildPrintDocument(
                        new[] { selectedLog },
                        "Selected import/export operation result.",
                        "Import / Export Selected Result");
                    new PrintPreviewWindow().ShowPreview(selectedDocument, "Import / Export Selected Result", "Review one selected data-operation result before copying, printing, or filing the handoff.");
                    return;
                }

                if (DataContext is not ImportExportViewModel vm || vm.ImportExportLogs.Count == 0)
                {
                    WpfMessageBox.Show("There are no import/export log rows to print.", "Import / Export", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var document = BuildPrintDocument(vm.ImportExportLogs.ToList(), vm.LogSummary);
                new PrintPreviewWindow().ShowPreview(document, "Import / Export Log", "Review the current session's import, export, image, backup, and restore results before staff handoff.");
            });
        }

        private static FlowDocument BuildPrintDocument(
            IReadOnlyCollection<string> logs,
            string summary,
            string title = "Import / Export Operation Log")
        {
            var safeLogs = logs?.Where(log => !string.IsNullOrWhiteSpace(log)).ToList() ?? new List<string>();
            var printedLogs = safeLogs.Take(MaxPrintedLogRows).ToList();
            var omittedLogCount = Math.Max(0, safeLogs.Count - printedLogs.Count);
            var document = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11,
                PagePadding = new Thickness(36),
                ColumnGap = 0,
                TextAlignment = TextAlignment.Left
            };

            document.Blocks.Add(new Paragraph(new Run(title))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Prepared {DateTime.Now:g}"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 2)
            });

            document.Blocks.Add(BuildSummarySection(title, summary, safeLogs.Count, printedLogs.Count, omittedLogCount));

            if (printedLogs.Count == 0)
            {
                document.Blocks.Add(new Paragraph(new Run("No import/export operation results were available when this packet was prepared."))
                {
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 8, 0, 0)
                });
                return document;
            }

            var table = new Table
            {
                CellSpacing = 0,
                Margin = new Thickness(0, 8, 0, 0)
            };
            table.Columns.Add(new TableColumn { Width = new GridLength(0.14, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.86, GridUnitType.Star) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "Entry", true);
            AddCell(header, "Operation Result", true);

            var number = 1;
            foreach (var log in printedLogs)
            {
                var row = new TableRow();
                rowGroup.Rows.Add(row);
                AddCell(row, number.ToString());
                AddCell(row, log.Trim());
                number++;
            }

            document.Blocks.Add(table);
            if (omittedLogCount > 0)
            {
                document.Blocks.Add(new Paragraph(new Run($"{omittedLogCount} additional log rows were omitted from this preview. Narrow the session handoff or print a selected row when the full run log is very large."))
                {
                    FontSize = 10,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 10, 0, 0)
                });
            }

            document.Blocks.Add(new Paragraph(new Run("Review skipped rows, failures, backup paths, restore notices, and omitted-row counts before clearing the in-app run log."))
            {
                FontSize = 10,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 10, 0, 0)
            });
            return document;
        }

        private static Section BuildSummarySection(string title, string summary, int logCount, int printedLogCount, int omittedLogCount)
        {
            var section = new Section
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 0, 0, 8),
                Margin = new Thickness(0, 6, 0, 8)
            };

            var table = new Table
            {
                Tag = "KeyValue",
                CellSpacing = 0,
                Margin = new Thickness(0)
            };
            table.Columns.Add(new TableColumn { Width = new GridLength(0.26, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.74, GridUnitType.Star) });

            var group = new TableRowGroup();
            table.RowGroups.Add(group);
            AddKeyValueRow(group, "Packet", title);
            AddKeyValueRow(group, "Visible Log Rows", logCount.ToString());
            AddKeyValueRow(group, "Printed Log Rows", printedLogCount.ToString());
            AddKeyValueRow(group, "Omitted Log Rows", omittedLogCount.ToString());
            AddKeyValueRow(group, "Session Summary", ValueOrNotRecorded(summary));

            section.Blocks.Add(table);
            return section;
        }

        private static void AddKeyValueRow(TableRowGroup group, string label, string value)
        {
            var row = new TableRow();
            group.Rows.Add(row);
            AddCell(row, label, true);
            AddCell(row, value);
        }

        private static void AddCell(TableRow row, string text, bool isHeader = false)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(text ?? string.Empty))
            {
                Margin = new Thickness(2),
                TextAlignment = TextAlignment.Left
            })
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 0.5),
                Padding = new Thickness(4, 3, 4, 3),
                FontWeight = isHeader ? FontWeights.SemiBold : FontWeights.Normal
            });
        }

        private static string ValueOrNotRecorded(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "Not recorded" : value.Trim();
    }
}
