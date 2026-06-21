using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Windows;
using WpfMessageBox = System.Windows.MessageBox;

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
                row.Focus();
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
            UiActionGuard.Run(this, "Activity Logs", () =>
            {
                if (ActivityGrid.SelectedItem is not ActivityLog log)
                {
                    WpfMessageBox.Show("Select an activity row first.", "Activity Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                DetailDialogWindow.ShowDialogFor(
                    Window.GetWindow(this),
                    "Activity Detail",
                    "Activity Detail",
                    FormatLogDetail(log),
                    "Review the selected audit trail, destination, and next action without losing row context.",
                    ActivityLogsViewModel.ClassifyAction(log.Action),
                    "Close returns to Activity Logs with the selected audit row still available for copy, print, or related-page routing.");
            });
        }

        private void OpenRelatedPage_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Activity Logs", () =>
            {
                if (ActivityGrid.SelectedItem is not ActivityLog log)
                {
                    WpfMessageBox.Show("Select an activity row first.", "Activity Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (Window.GetWindow(this)?.DataContext is not MainViewModel main)
                {
                    WpfMessageBox.Show("The related page is not available from this window.", "Activity Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                switch (ActivityLogsViewModel.BuildDestinationKey(log.Action))
                {
                    case "Rentals":
                        main.OpenRentalsCommand.Execute(null);
                        break;
                    case "Reservations":
                        main.OpenReservationsCommand.Execute(null);
                        break;
                    case "Calibration":
                        main.OpenCalibrationCommand.Execute(null);
                        break;
                    case "Maintenance":
                        main.OpenMaintenanceCommand.Execute(null);
                        break;
                    case "ImportExport":
                        main.OpenImportExportCommand.Execute(null);
                        break;
                    case "Users":
                        main.OpenUsersCommand.Execute(null);
                        break;
                    case "Categories":
                        main.OpenCategoriesCommand.Execute(null);
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
            });
        }

        private void CopySelectedLog_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Activity Logs", () =>
            {
                if (ActivityGrid.SelectedItem is not ActivityLog log)
                {
                    WpfMessageBox.Show("Select an activity row first.", "Activity Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                System.Windows.Clipboard.SetText(FormatLogDetail(log));
            });
        }

        private void PrintLogs_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Activity Logs", () =>
            {
                if (DataContext is not ActivityLogsViewModel vm || vm.FilteredLogs.Count == 0)
                {
                    WpfMessageBox.Show("There are no activity rows to print.", "Activity Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var document = BuildPrintDocument(vm.FilteredLogs.ToList(), vm.StatusMessage, vm.ActivitySummary);
                new PrintPreviewWindow().ShowPreview(
                    document,
                    "Activity Logs",
                    "Review the filtered audit trail, destination routing, and operator handoff before printing.");
            });
        }

        private static string FormatLogDetail(ActivityLog log)
        {
            var destinationKey = ActivityLogsViewModel.BuildDestinationKey(log.Action);
            return $"Timestamp: {log.Timestamp:g}{Environment.NewLine}" +
                   $"User: {SafeText(log.UserName, "Unknown user")} (ID {log.UserID}){Environment.NewLine}" +
                   $"Type: {ActivityLogsViewModel.ClassifyAction(log.Action)}{Environment.NewLine}" +
                   $"Destination: {ActivityLogsViewModel.BuildDestinationName(destinationKey)}{Environment.NewLine}" +
                   $"Next action: {ActivityLogsViewModel.BuildNextAction(log.Action)}{Environment.NewLine}" +
                   $"Action: {SafeText(log.Action, "No activity detail was recorded.")}";
        }

        private static FlowDocument BuildPrintDocument(IReadOnlyCollection<ActivityLog> logs, string summary, string activitySummary)
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
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run(activitySummary))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var table = new Table { CellSpacing = 0 };
            foreach (var width in new[] { 115.0, 105.0, 100.0, 105.0, 275.0 })
                table.Columns.Add(new TableColumn { Width = new GridLength(width) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "Timestamp");
            AddCell(header, "User");
            AddCell(header, "Type");
            AddCell(header, "Destination");
            AddCell(header, "Action");

            foreach (var log in logs)
            {
                var row = new TableRow();
                rowGroup.Rows.Add(row);
                AddCell(row, log.Timestamp.ToString("g"));
                AddCell(row, SafeText(log.UserName, "Unknown user"));
                AddCell(row, ActivityLogsViewModel.ClassifyAction(log.Action));
                AddCell(row, ActivityLogsViewModel.BuildDestinationName(ActivityLogsViewModel.BuildDestinationKey(log.Action)));
                AddCell(row, SafeText(log.Action, "No activity detail was recorded."));
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

        private static string SafeText(string? text, string fallback)
        {
            return string.IsNullOrWhiteSpace(text) ? fallback : text;
        }
    }
}
