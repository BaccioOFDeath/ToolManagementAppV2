using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Windows;
using WpfMessageBox = System.Windows.MessageBox;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ActivityLogsPage : Page
    {
        private const int MaxActivityPrintRows = 250;
        private bool _hasLoadedLogs;
        private ActivityLogsViewModel? _loadedViewModel;

        public ActivityLogsPage()
        {
            InitializeComponent();
            Loaded += ActivityLogsPage_Loaded;
            DataContextChanged += ActivityLogsPage_DataContextChanged;
            PreviewKeyDown += ActivityLogsPage_PreviewKeyDown;
        }

        private async void ActivityLogsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ActivityLogsViewModel vm)
                return;

            ActivitySearchBox.Focus();

            if (_hasLoadedLogs && ReferenceEquals(_loadedViewModel, vm))
                return;

            if (!vm.RefreshCommand.CanExecute(null))
                return;

            await Dispatcher.Yield(DispatcherPriority.Background);
            if (!ReferenceEquals(DataContext, vm) || !vm.RefreshCommand.CanExecute(null))
                return;

            _loadedViewModel = vm;
            _hasLoadedLogs = await vm.LoadLogsAsync();
        }

        private void ActivityLogsPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(e.NewValue, _loadedViewModel))
            {
                _hasLoadedLogs = false;
                _loadedViewModel = e.NewValue as ActivityLogsViewModel;
            }
        }

        private void ActivityGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsActivityDirectoryBusy())
            {
                e.Handled = true;
                return;
            }

            RetargetActivitySelectionFromEvent(e);
            OpenSelectedLog_Click(sender, e);
            e.Handled = true;
        }

        private void ActivityGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsActivityDirectoryBusy())
            {
                e.Handled = true;
                return;
            }

            GridContextMenuSelection.SelectRow(sender, e);
        }

        private async void RefreshLogs_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ActivityLogsViewModel vm && vm.RefreshCommand.CanExecute(null))
            {
                _hasLoadedLogs = false;
                var loaded = await vm.LoadLogsAsync();
                _hasLoadedLogs = loaded;
                _loadedViewModel = vm;
            }
        }

        private void ActivityLogsPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                ActivitySearchBox.Focus();
                ActivitySearchBox.SelectAll();
                e.Handled = true;
                return;
            }

            if (IsTextEditingElement(e.OriginalSource) || !IsActivityLogShortcut(e))
                return;

            if (IsActivityDirectoryBusy())
            {
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R)
            {
                RefreshLogs_Click(sender, e);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
            {
                OpenRelatedPage_Click(sender, e);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                CopySelectedLog_Click(sender, e);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                PrintLogs_Click(sender, e);
                e.Handled = true;
                return;
            }

            OpenSelectedLog_Click(sender, e);
            e.Handled = true;
        }

        private void OpenSelectedLog_Click(object sender, RoutedEventArgs e)
        {
            UiActionGuard.Run(this, "Activity Logs", () =>
            {
                if (DataContext is ActivityLogsViewModel { IsBusy: true })
                {
                    WpfMessageBox.Show("Wait for the activity directory to finish updating before opening details.", "Activity Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var log = GetSelectedActivityLogForAction();
                if (log == null)
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
                if (DataContext is ActivityLogsViewModel { IsBusy: true })
                {
                    WpfMessageBox.Show("Wait for the activity directory to finish updating before opening the related page.", "Activity Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var log = GetSelectedActivityLogForAction();
                if (log == null)
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
                if (DataContext is ActivityLogsViewModel { IsBusy: true })
                {
                    WpfMessageBox.Show("Wait for the activity directory to finish updating before copying a handoff.", "Activity Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var log = GetSelectedActivityLogForAction();
                if (log == null)
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

                if (vm.IsBusy)
                {
                    WpfMessageBox.Show("Wait for the activity directory to finish updating before opening print preview.", "Activity Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var totalFilteredCount = vm.FilteredLogs.Count;
                var printRows = vm.FilteredLogs.Take(MaxActivityPrintRows).ToList();
                var document = BuildPrintDocument(printRows, totalFilteredCount, vm.PrintStatusText, vm.ActivitySummary);
                new PrintPreviewWindow().ShowPreview(
                    document,
                    "Activity Logs",
                    "Review the filtered audit trail, destination routing, and operator handoff before printing. Large result sets print the first 250 rows so preview stays responsive.");
            });
        }

        private bool IsActivityDirectoryBusy() => DataContext is ActivityLogsViewModel { IsBusy: true };

        private void RetargetActivitySelectionFromEvent(RoutedEventArgs e)
        {
            var row = FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row?.Item is not ActivityLog log)
                return;

            ActivityGrid.SelectedItem = log;
            ActivityGrid.ScrollIntoView(log);
            if (DataContext is ActivityLogsViewModel vm)
                vm.SelectedLog = log;
        }

        private ActivityLog? GetSelectedActivityLogForAction()
        {
            if (ActivityGrid.SelectedItem is ActivityLog gridLog)
                return gridLog;

            return DataContext is ActivityLogsViewModel vm
                ? vm.SelectedLog
                : null;
        }

        private static bool IsActivityLogShortcut(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
                return e.Key is Key.R or Key.O or Key.D or Key.C or Key.P;

            return Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter;
        }

        private static bool IsTextEditingElement(object? source)
        {
            return source is TextBox or ComboBox;
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T match)
                    return match;

                current = TryGetParent(current);
            }

            return null;
        }

        private static DependencyObject? TryGetParent(DependencyObject current)
        {
            try
            {
                return System.Windows.Media.VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current);
            }
            catch (InvalidOperationException)
            {
                return LogicalTreeHelper.GetParent(current);
            }
            catch (ArgumentException)
            {
                return LogicalTreeHelper.GetParent(current);
            }
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

        private static FlowDocument BuildPrintDocument(IReadOnlyCollection<ActivityLog> logs, int totalFilteredCount, string summary, string activitySummary)
        {
            var printedRowCount = logs.Count;
            var omittedRowCount = Math.Max(0, totalFilteredCount - printedRowCount);
            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 10,
                PagePadding = new Thickness(36)
            };

            document.Blocks.Add(new Paragraph(new Run("Activity Logs"))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g}"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(BuildSummarySection(summary, activitySummary, totalFilteredCount, printedRowCount, omittedRowCount));

            var table = new Table { CellSpacing = 0 };
            table.Columns.Add(new TableColumn { Width = new GridLength(0.16, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.16, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.16, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.18, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(0.34, GridUnitType.Star) });

            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            var header = new TableRow { FontWeight = FontWeights.SemiBold };
            rowGroup.Rows.Add(header);
            AddCell(header, "When / User", true);
            AddCell(header, "Type", true);
            AddCell(header, "Destination", true);
            AddCell(header, "Next Action", true);
            AddCell(header, "Activity Detail", true);

            if (printedRowCount == 0)
            {
                var emptyRow = new TableRow();
                rowGroup.Rows.Add(emptyRow);
                AddCell(emptyRow, "No activity rows matched the current print packet.", false, 5);
            }
            else
            {
                foreach (var log in logs)
                {
                    var destinationKey = ActivityLogsViewModel.BuildDestinationKey(log.Action);
                    var row = new TableRow();
                    rowGroup.Rows.Add(row);
                    AddCell(row, $"{log.Timestamp:g}\n{SafeText(log.UserName, "Unknown user")} (ID {log.UserID})");
                    AddCell(row, ActivityLogsViewModel.ClassifyAction(log.Action));
                    AddCell(row, ActivityLogsViewModel.BuildDestinationName(destinationKey));
                    AddCell(row, ActivityLogsViewModel.BuildNextAction(log.Action));
                    AddCell(row, SafeText(log.Action, "No activity detail was recorded."));
                }
            }

            document.Blocks.Add(table);
            document.Blocks.Add(new Paragraph(new Run("Review destination, next action, and any omitted rows before filing the audit handoff."))
            {
                FontSize = 9,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 8, 0, 0)
            });
            return document;
        }

        private static Block BuildSummarySection(string summary, string activitySummary, int totalFilteredCount, int printedRowCount, int omittedRowCount)
        {
            var group = new Section
            {
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 0.5),
                Padding = new Thickness(0, 0, 0, 8),
                Margin = new Thickness(0, 0, 0, 10)
            };

            AddSummaryLine(group, "Print Packet", $"{printedRowCount} of {totalFilteredCount} filtered activity row(s)");
            AddSummaryLine(group, "Omitted Rows", omittedRowCount == 0 ? "None" : $"{omittedRowCount} row(s) not printed; narrow filters or print again after refining the audit search.");
            AddSummaryLine(group, "Filter Status", ValueOrNotRecorded(summary));
            AddSummaryLine(group, "Activity Mix", ValueOrNotRecorded(activitySummary));
            return group;
        }

        private static void AddSummaryLine(Section group, string label, string value)
        {
            var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 2) };
            paragraph.Inlines.Add(new Run($"{label}: ") { FontWeight = FontWeights.SemiBold });
            paragraph.Inlines.Add(new Run(value));
            group.Blocks.Add(paragraph);
        }

        private static void AddCell(TableRow row, string text, bool isHeader = false, int columnSpan = 1)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(text ?? string.Empty))
            {
                Margin = new Thickness(2)
            })
            {
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 0.5),
                ColumnSpan = columnSpan,
                FontWeight = isHeader ? FontWeights.SemiBold : FontWeights.Normal,
                Padding = new Thickness(3, 2, 3, 2)
            });
        }

        private static string ValueOrNotRecorded(string? text)
        {
            return SafeText(text, "Not recorded");
        }

        private static string SafeText(string? text, string fallback)
        {
            return string.IsNullOrWhiteSpace(text) ? fallback : text.Trim();
        }
    }
}
