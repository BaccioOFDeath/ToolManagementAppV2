using System.Windows.Controls;
using System.Windows.Input;
using TextBox = System.Windows.Controls.TextBox;

namespace DeviceManagementApp.Views.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        void PasswordIterationsBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var proposed = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !int.TryParse(proposed, out var value) || value <= 0;
        }

        void AutoLogoutMinutesBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var proposed = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !int.TryParse(proposed, out var value) || value < 0;
        }
    }
}
