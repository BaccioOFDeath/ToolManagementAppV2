using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.Extensions;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Windows
{
    public partial class CheckoutHistoryWindow : Window
    {
        const int MaxVisibleHistoryRows = 500;

        public CheckoutHistoryWindow(ItemModel item, IEnumerable<ActivityLog> logs)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(logs);

            var orderedLogs = logs
                .OrderByDescending(log => log.Timestamp)
                .ToList();

            ItemSummaryText = BuildItemSummary(item);
            TotalLogCount = orderedLogs.Count;
            VisibleLogs = new ObservableCollection<ActivityLog>(orderedLogs.Take(MaxVisibleHistoryRows));
            VisibleLogCount = VisibleLogs.Count;
            OmittedLogCount = Math.Max(0, TotalLogCount - VisibleLogCount);
            HasOmittedLogs = OmittedLogCount > 0;
            OmittedLogSummary = HasOmittedLogs
                ? $"Showing the first {VisibleLogCount:N0} newest checkout history rows. {OmittedLogCount:N0} older rows are omitted from this dialog to keep review responsive."
                : string.Empty;
            FooterStatusText = TotalLogCount == 0
                ? "No checkout or check-in history rows were returned for this item."
                : $"Showing {VisibleLogCount:N0} of {TotalLogCount:N0} checkout history rows, newest first.";

            InitializeComponent();
            this.UseResponsiveDefaultSize(820, 620);
            DataContext = this;
            Loaded += (_, _) => CheckoutHistoryGrid.Focus();
        }

        public string ItemSummaryText { get; }
        public ObservableCollection<ActivityLog> VisibleLogs { get; }
        public int TotalLogCount { get; }
        public int VisibleLogCount { get; }
        public int OmittedLogCount { get; }
        public bool HasOmittedLogs { get; }
        public string OmittedLogSummary { get; }
        public string FooterStatusText { get; }

        void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        void CheckoutHistoryWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        static string BuildItemSummary(ItemModel item)
        {
            var number = string.IsNullOrWhiteSpace(item.ItemNumber) ? "Not recorded" : item.ItemNumber;
            var name = string.IsNullOrWhiteSpace(item.Name) ? "Unnamed item" : item.Name;
            return $"{number} - {name}";
        }
    }
}
