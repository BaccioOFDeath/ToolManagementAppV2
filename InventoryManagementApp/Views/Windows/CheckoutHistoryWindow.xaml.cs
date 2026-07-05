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
        const int MaxLoadedHistoryRows = MaxVisibleHistoryRows + 1;

        public CheckoutHistoryWindow(ItemModel item, IEnumerable<ActivityLog> logs)
        {
            ArgumentNullException.ThrowIfNull(item);
            ArgumentNullException.ThrowIfNull(logs);

            var orderedLogs = logs
                .OrderByDescending(log => log.Timestamp)
                .Take(MaxLoadedHistoryRows)
                .ToList();

            ItemSummaryText = BuildItemSummary(item);
            TotalLogCount = orderedLogs.Count;
            VisibleLogs = new ObservableCollection<ActivityLog>(orderedLogs.Take(MaxVisibleHistoryRows));
            VisibleLogCount = VisibleLogs.Count;
            HasOmittedLogs = TotalLogCount > VisibleLogCount;
            OmittedLogCount = HasOmittedLogs ? 1 : 0;
            OlderHistoryIndicator = HasOmittedLogs ? "Yes" : "No";
            OmittedLogSummary = HasOmittedLogs
                ? $"Showing the newest {VisibleLogCount:N0} checkout history rows. At least one older checkout history row exists outside this responsive review set."
                : string.Empty;
            FooterStatusText = TotalLogCount == 0
                ? "No checkout or check-in history rows were returned for this item."
                : HasOmittedLogs
                    ? $"Showing newest {VisibleLogCount:N0} checkout history rows; more older rows are available in the audit trail."
                    : $"Showing {VisibleLogCount:N0} checkout history row{(VisibleLogCount == 1 ? string.Empty : "s")}, newest first.";

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
        public string OlderHistoryIndicator { get; }
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