using System;

namespace InventoryManagementApp.Services.Dashboard
{
    public sealed class DashboardFeedEntry
    {
        public DashboardFeedEntry(DateOnly date, decimal amount)
        {
            Date = date;
            Amount = amount;
        }

        public DateOnly Date { get; }
        public decimal Amount { get; }
    }
}
