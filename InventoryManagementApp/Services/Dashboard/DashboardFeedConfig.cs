using System;

namespace InventoryManagementApp.Services.Dashboard
{
    public sealed class DashboardFeedConfig
    {
        public string DateColumn { get; init; } = "DATE";
        public string AmountColumn { get; init; } = string.Empty;
        public string? JobNumberColumn { get; init; }
            = null;
        public string? InvoiceNumberColumn { get; init; }
            = null;
        public DateOnly? StartDate { get; init; }
            = null;
        public DateOnly? EndDate { get; init; }
            = null;
    }
}
