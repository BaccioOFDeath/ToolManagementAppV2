using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using WpfMessageBox = System.Windows.MessageBox;
using WpfPrintDialog = System.Windows.Controls.PrintDialog;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ActivityLogsPage : Page
    {
        public ActivityLogsPage()
        {
            InitializeComponent();
            Loaded += ActivityLogsPage_Loaded;
        }

        private async void ActivityLogsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ActivityLogsViewModel vm)
            {
                await vm.LoadLogsAsync();
            }
        }

        private void ActivityGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedLog_Click(sender, e);
        }

        private void ActivityGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.IsSelected = true;
                e.Handled = true;
            }
        }

        private async void RefreshLogs_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ActivityLogsViewModel vm)
            {
                await vm.LoadLogsAsync();
            }
        }

        private void OpenSelectedLog_Click(object sender, RoutedEventArgs e)
        {
            if (ActivityGrid.SelectedItem is not ActivityLog log)
            {
                WpfMessageBox.Show("Select an activity row first.", "Activity Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            WpfMessageBox.Show(FormatLogDetail(log), "Activity Detail", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopySelectedLog_Click(object sender, RoutedEventArgs e)
        {
            if (ActivityGrid.SelectedItem is not ActivityLog log)
            {
                WpfMessageBox.Show("Select an activity row first.", "Activity Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            System.Windows.Clipboard.SetText(FormatLogDetail(log));
        }

        private void PrintLogs_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ActivityLogsViewModel vm || vm.FilteredLogs.Count == 0)
            {
                WpfMessageBox.Show("There are no activity rows to print.", "Activity Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var printDialog = new WpfPrintDialog();
            if (printDialog.ShowDialog() != true)
                return;

            var document = BuildPrintDocument(vm.FilteredLogs.ToList(), vm.StatusMessage);
            document.PageWidth = printDialog.PrintableAreaWidth;
            document.PageHeight = printDialog.PrintableAreaHeight;
            document.PagePadding = new Thickness(36);
            document.ColumnWidth = printDialog.PrintableAreaWidth;
            printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "Activity Logs");
        }

        private static string FormatLogDetail(ActivityLog log)
        {
            return $"Timestamp: {log.Timestamp:g}{Environment.NewLine}" +
                   $"User: {log.UserName} (ID {log.UserID}){Environment.NewLine}" +
                   $"Type: {ActivityLogsViewModel.ClassifyAction(log.Action)}{Environment.NewLine}" +
                   $"Action: {log.Action}";
        }

        private static FlowDocument BuildPrintDocument(IReadOnlyCollection<ActivityLog> logs, string summary)
        {
            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 10
            };

            document.Blocks.Add(new Paragraph(new Run("Activity Logs"))
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
            foreach (var width in new[] { 125.0, 120.0, 115.0, 420.0 })
                table.Columns.Add(new TableColumn { Width = new GridLength(width) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "Timestamp");
            AddCell(header, "User");
            AddCell(header, "Type");
            AddCell(header, "Action");

            foreach (var log in logs)
            {
                var row = new TableRow();
                rowGroup.Rows.Add(row);
                AddCell(row, log.Timestamp.ToString("g"));
                AddCell(row, log.UserName);
                AddCell(row, ActivityLogsViewModel.ClassifyAction(log.Action));
                AddCell(row, log.Action);
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
