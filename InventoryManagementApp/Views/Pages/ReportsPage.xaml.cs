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
    public partial class ReportsPage : Page
    {
        private const int MaxReportPrintRows = 250;

        public ReportsPage()
        {
            InitializeComponent();
        }

        private void ReportGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ReportsViewModel { CanUseReportRows: true })
            {
                UiActionGuard.Run(this, "Reports", OpenSelectedDestination);
            }
        }

        private void OpenSourcePage_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Reports", OpenSelectedDestination);
        }

        private void ReportGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ReportsViewModel { IsBusy: true })
            {
                e.Handled = true;
                return;
            }

            GridContextMenuSelection.SelectRow(sender, e);
        }

        private void CopySelectedRow_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Reports", () =>
            {
                if (DataContext is ReportsViewModel { IsBusy: true })
                {
                    WpfMessageBox.Show("Wait for the report to finish generating before copying a handoff.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var line = GetSelectedReportLineForAction();
                if (line == null)
                {
                    WpfMessageBox.Show("Select a report row first.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                System.Windows.Clipboard.SetText(FormatHandoff(line));
            });
        }

        private void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Reports", () =>
            {
                if (DataContext is not ReportsViewModel vm)
                {
                    WpfMessageBox.Show("Run a report before printing.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (vm.IsBusy)
                {
                    WpfMessageBox.Show("Wait for the report to finish generating before opening print preview.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!vm.CanPrintCurrentReport)
                {
                    WpfMessageBox.Show("Run a report before printing.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var totalLineCount = vm.ReportLines.Count;
                var printRows = vm.ReportLines.Take(MaxReportPrintRows).ToList();
                var document = BuildReportDocument(vm.ReportTitle, vm.ReportSummary, vm.LastRunText, printRows, totalLineCount);
                new PrintPreviewWindow().ShowPreview(
                    document,
                    vm.ReportTitle,
                    "Review the report summary, destination routing, next-action handoff, and any omitted rows before printing. Large reports print the first 250 rows so preview stays responsive.");
            });
        }

        private void OpenSelectedDestination()
        {
            if (DataContext is ReportsViewModel { IsBusy: true })
            {
                WpfMessageBox.Show("Wait for the report to finish generating before opening a source page.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var line = GetSelectedReportLineForAction();
            if (line == null || string.IsNullOrWhiteSpace(line.DestinationKey))
            {
                WpfMessageBox.Show("Run a report and select a row first.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (Window.GetWindow(this)?.DataContext is not MainViewModel main)
            {
                WpfMessageBox.Show("The destination page is not available from this window.", "Reports", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            switch (line.DestinationKey)
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

        private ReportLine? GetSelectedReportLineForAction()
        {
            if (ReportGrid.SelectedItem is ReportLine gridLine)
                return gridLine;

            return DataContext is ReportsViewModel vm
                ? vm.SelectedReportLine
                : null;
        }

        private static string FormatHandoff(ReportLine line)
        {
            return $"{line.Category}: {line.Text}{Environment.NewLine}" +
                   $"Next action: {line.ActionHint}{Environment.NewLine}" +
                   $"Destination: {line.DestinationName}";
        }

        private static FlowDocument BuildReportDocument(string title, string summary, string lastRunText, IReadOnlyCollection<ReportLine> lines, int totalLineCount)
        {
            var safeLines = lines?.Where(line => line != null).ToList() ?? new List<ReportLine>();
            var safeTitle = ValueOrNotRecorded(title);
            var printedLineCount = safeLines.Count;
            var omittedLineCount = Math.Max(0, totalLineCount - printedLineCount);
            var document = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11,
                PagePadding = new Thickness(36),
                ColumnGap = 0,
                TextAlignment = TextAlignment.Left
            };

            document.Blocks.Add(new Paragraph(new Run(safeTitle))
            {
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Bold(new Run("REPORT HANDOFF")))
            {
                FontSize = 14,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12)
            });
            document.Blocks.Add(new Paragraph(new Run($"Prepared {DateTime.Now:g}"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 2)
            });

            document.Blocks.Add(BuildSummarySection(safeTitle, summary, lastRunText, totalLineCount, printedLineCount, omittedLineCount));

            if (safeLines.Count == 0)
            {
                document.Blocks.Add(new Paragraph(new Run("No report rows were available when this packet was prepared."))
                {
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 8, 0, 0)
                });
                return document;
            }

            var table = new Table
            {
                CellSpacing = 0,
                Margin = new Thickness(0, 8, 0, 0),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };
            table.Columns.Add(new TableColumn { Width = new GridLength(0.08, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.16, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.18, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.36, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.22, GridUnitType.Star) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow
            {
                FontWeight = FontWeights.SemiBold,
                Background = System.Windows.Media.Brushes.LightGray
            };
            rowGroup.Rows.Add(header);
            AddCell(header, "Entry", true);
            AddCell(header, "Type", true);
            AddCell(header, "Destination", true);
            AddCell(header, "Report Detail", true);
            AddCell(header, "Next Action", true);

            foreach (var line in safeLines)
            {
                var row = new TableRow();
                if (line.Number % 2 == 0)
                    row.Background = Brushes.WhiteSmoke;
                rowGroup.Rows.Add(row);
                AddCell(row, line.Number.ToString());
                AddCell(row, ValueOrNotRecorded(line.Category));
                AddCell(row, ValueOrNotRecorded(line.DestinationName));
                AddCell(row, ValueOrNotRecorded(line.Text));
                AddCell(row, ValueOrNotRecorded(line.ActionHint));
            }

            document.Blocks.Add(table);
            document.Blocks.Add(new Paragraph(new Run("Review each destination, source-page route, next action, and omitted-row count before closing the report packet or assigning follow-up work."))
            {
                FontSize = 10,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 10, 0, 0)
            });
            return document;
        }

        private static Section BuildSummarySection(string title, string summary, string lastRunText, int totalLineCount, int printedLineCount, int omittedLineCount)
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
            table.Columns.Add(new TableColumn { Width = new GridLength(0.24, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.76, GridUnitType.Star) });

            var group = new TableRowGroup();
            table.RowGroups.Add(group);
            AddKeyValueRow(group, "Report", title);
            AddKeyValueRow(group, "Total Action Rows", totalLineCount.ToString());
            AddKeyValueRow(group, "Printed Action Rows", printedLineCount.ToString());
            AddKeyValueRow(group, "Omitted Action Rows", omittedLineCount == 0 ? "None" : $"{omittedLineCount} rows omitted to keep preview responsive");
            AddKeyValueRow(group, "Large Report Limit", $"First {MaxReportPrintRows} action rows");
            AddKeyValueRow(group, "Last Run", ValueOrNotRecorded(lastRunText));
            AddKeyValueRow(group, "Summary", ValueOrNotRecorded(summary));

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
                Padding = new Thickness(5, 3, 5, 3),
                FontWeight = isHeader ? FontWeights.SemiBold : FontWeights.Normal
            });
        }

        private static string ValueOrNotRecorded(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "Not recorded" : value.Trim();
    }
}