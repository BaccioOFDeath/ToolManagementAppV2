using System;

namespace InventoryManagementApp.Models.Domain
{
    public class MonthlyTarget
    {
        public int TargetId { get; set; }
        public int FinancialYearStart { get; set; }
        public int MonthOffset { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TargetAmount { get; set; }

        public DateTime GetMonthDate() => new DateTime(Year, Month, 1);
    }
}
