using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Windows;
using WpfMessageBox = System.Windows.MessageBox;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
        }

        private void ReportGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedDestination();
        }

        private void OpenSourcePage_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedDestination();
        }

        private void ReportGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = FindParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row != null && !row.IsSelected)
                row.IsSelected = true;
        }

        private void CopySelectedRow_Click(object sender, RoutedEventArgs e)
        {
            if (ReportGrid.SelectedItem is not ReportLine line)
            {
                WpfMessageBox.Show("Select a report row first.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            System.Windows.Clipboard.SetText(FormatHandoff(line));
        }

        private void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ReportsViewModel vm || vm.ReportLines.Count == 0)
            {
                WpfMessageBox.Show("Run a report before printing.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var document = BuildReportDocument(vm.ReportTitle, vm.ReportSummary, vm.LastRunText, vm.ReportLines.ToList());
            new PrintPreviewWindow().ShowPreview(
                document,
                vm.ReportTitle,
                "Review the report summary, destination routing, and next-action handoff before printing.");
        }

        private void OpenSelectedDestination()
        {
            var line = ReportGrid.SelectedItem as ReportLine;
            var key = line?.DestinationKey;
            if (string.IsNullOrWhiteSpace(key) && DataContext is ReportsViewModel vm)
                key = vm.SelectedLineDestinationKey;

            if (string.IsNullOrWhiteSpace(key))
            {
                WpfMessageBox.Show("Run a report and select a row first.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (Window.GetWindow(this)?.DataContext is not MainViewModel main)
            {
                WpfMessageBox.Show("The destination page is not available from this window.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            switch (key)
            {
                case "ActivityLogs":
                    main.OpenActivityLogsCommand.Execute(null);
                    break;
                case "Customers":
                    main.OpenCustomersCommand.Execute(null);
                    break;
                case "Users":
                    main.OpenUsersCommand.Execute(null);
                    break;
                case "Rentals":
                    main.OpenRentalsCommand.Execute(null);
                    break;
                case "Reservations":
                    main.OpenReservationsCommand.Execute(null);
                    break;
                case "Maintenance":
                    main.OpenMaintenanceCommand.Execute(null);
                    break;
                case "Calibration":
                    main.OpenCalibrationCommand.Execute(null);
                    break;
                case "Kits":
                    main.OpenKitManagementCommand.Execute(null);
                    break;
                case "Items":
                    main.OpenManageItemsCommand.Execute(null);
                    break;
                default:
                    main.OpenDashboardCommand.Execute(null);
                    break;
            }
        }

        private static string FormatHandoff(ReportLine line)
        {
            return $"{line.Category}: {line.Text}{Environment.NewLine}" +
                   $"Next action: {line.ActionHint}{Environment.NewLine}" +
                   $"Destination: {line.DestinationName}";
        }

        private static FlowDocument BuildReportDocument(string title, string summary, string lastRunText, IReadOnlyCollection<ReportLine> lines)
        {
            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 11
            };

            document.Blocks.Add(new Paragraph(new Run(title))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g} - Last run {lastRunText} - {lines.Count} row(s)"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run(summary))
            {
                Margin = new Thickness(0, 0, 0, 10)
            });

            var table = new Table { CellSpacing = 0 };
            foreach (var width in new[] { 45.0, 85.0, 105.0, 300.0, 205.0 })
                table.Columns.Add(new TableColumn { Width = new GridLength(width) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "#");
            AddCell(header, "Type");
            AddCell(header, "Destination");
            AddCell(header, "Report Detail");
            AddCell(header, "Next Action");

            foreach (var line in lines)
            {
                var row = new TableRow();
                rowGroup.Rows.Add(row);
                AddCell(row, line.Number.ToString());
                AddCell(row, line.Category);
                AddCell(row, line.DestinationName);
                AddCell(row, line.Text);
                AddCell(row, line.ActionHint);
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

        private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = child is Popup popup
                    ? popup.PlacementTarget
                    : System.Windows.Media.VisualTreeHelper.GetParent(child);
            }

            return null;
        }
    }
}