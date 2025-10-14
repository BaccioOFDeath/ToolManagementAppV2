using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace InventoryManagementApp.ViewModels
{
    public partial class MonthlyTargetEntryViewModel : ObservableObject
    {
        [ObservableProperty]
        private string monthDisplay = string.Empty;

        [ObservableProperty]
        private int month;

        [ObservableProperty]
        private int year;

        [ObservableProperty]
        private int monthOffset;

        [ObservableProperty]
        private decimal targetAmount;

        public void UpdateDisplay()
        {
            MonthDisplay = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month)} {Year}";
        }
    }
}
