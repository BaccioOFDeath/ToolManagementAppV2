using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Windows;
using WpfMessageBox = System.Windows.MessageBox;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ImportExportPage : Page
    {
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
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.IsSelected = true;
                e.Handled = true;
            }
        }

        private void OpenSelectedLog_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Import / Export", () =>
            {
                if (ImportExportLogGrid.SelectedItem is not string log || string.IsNullOrWhiteSpace(log))
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
                if (ImportExportLogGrid.SelectedItem is not string log || string.IsNullOrWhiteSpace(log))
                {
                    WpfMessageBox.Show("Select an import/export log row first.", "Import / Export", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                System.Windows.Clipboard.SetText(log);
            });
        }

        private void PrintLogs_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Import / Export", () =>
            {
                if (DataContext is not ImportExportViewModel vm || vm.ImportExportLogs.Count == 0)
                {
                    WpfMessageBox.Show("There are no import/export log rows to print.", "Import / Export", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var document = BuildPrintDocument(vm.ImportExportLogs.ToList(), vm.LogSummary);
                new PrintPreviewWindow().ShowPreview(document, "Import / Export Log", null);
            });
        }

        private static FlowDocument BuildPrintDocument(IReadOnlyCollection<string> logs, string summary)
        {
            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 11
            };

            document.Blocks.Add(new Paragraph(new Run("Import / Export Operation Log"))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g} - {summary}"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var table = new Table { CellSpacing = 0 };
            table.Columns.Add(new TableColumn { Width = new GridLength(55) });
            table.Columns.Add(new TableColumn { Width = new GridLength(680) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "#");
            AddCell(header, "Result");

            var number = 1;
            foreach (var log in logs)
            {
                var row = new TableRow();
                rowGroup.Rows.Add(row);
                AddCell(row, number.ToString());
                AddCell(row, log);
                number++;
            }

            document.Blocks.Add(table);
            return document;
        }

        private static void AddCell(TableRow row, string text)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(text ?? string.Empty))
            {
                Margin = new Thickness(2)
            })
            {
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 0.5),
                Padding = new Thickness(3, 2, 3, 2)
            });
        }
    }
}
