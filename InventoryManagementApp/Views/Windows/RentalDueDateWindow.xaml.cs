using System;
using System.Windows;

namespace InventoryManagementApp.Views.Windows
{
    public partial class RentalDueDateWindow : Window
    {
        readonly TimeSpan _dueTime;

        public RentalDueDateWindow(DateTime currentDueDate)
        {
            InitializeComponent();
            _dueTime = currentDueDate.TimeOfDay;
            CurrentDueText.Text = currentDueDate.ToString("yyyy-MM-dd HH:mm");
            DueDatePicker.SelectedDate = currentDueDate.Date;
        }

        public DateTime SelectedDueDate { get; private set; }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DueDatePicker.SelectedDate is not DateTime selectedDate)
                return;

            SelectedDueDate = selectedDate.Date + _dueTime;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
