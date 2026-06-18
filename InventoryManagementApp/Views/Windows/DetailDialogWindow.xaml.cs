using System.Windows;

namespace InventoryManagementApp.Views.Windows
{
    /// <summary>
    /// Polished read-only detail surface for selected-row handoffs.
    /// </summary>
    public partial class DetailDialogWindow : Window
    {
        public DetailDialogWindow(string windowTitle, string header, string message, string? subhead = null, string? eyebrow = null, string? footer = null)
        {
            InitializeComponent();

            Title = string.IsNullOrWhiteSpace(windowTitle) ? "Detail" : windowTitle;
            HeaderText.Text = string.IsNullOrWhiteSpace(header) ? Title : header;
            MessageText.Text = message ?? string.Empty;
            SubheadText.Text = string.IsNullOrWhiteSpace(subhead)
                ? "Review the selected row context before returning to the current workflow."
                : subhead;
            EyebrowText.Text = string.IsNullOrWhiteSpace(eyebrow) ? "Detail" : eyebrow;
            FooterText.Text = string.IsNullOrWhiteSpace(footer)
                ? "Close returns to the current screen with the same row context."
                : footer;
        }

        public static void ShowDialogFor(Window? owner, string windowTitle, string header, string message, string? subhead = null, string? eyebrow = null, string? footer = null)
        {
            var dialog = new DetailDialogWindow(windowTitle, header, message, subhead, eyebrow, footer);
            if (owner != null)
                dialog.Owner = owner;

            dialog.ShowDialog();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
