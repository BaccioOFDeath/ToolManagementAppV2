using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using DeviceManagementApp.ViewModels;
using TextBox = System.Windows.Controls.TextBox;

namespace DeviceManagementApp.Views.Pages
{
    public partial class DeviceSettingsPage : Page
    {
        public DeviceSettingsPage()
        {
            InitializeComponent();
            Loaded += async (_, __) =>
            {
                if (DataContext is DeviceSettingsViewModel vm)
                {
                    await vm.InitializeAsync();
                }
            };
        }

        void PortsBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9,\\s]");
        }

        void AdditionalPortsBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9:,\\s]");
        }

        void TimeoutBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var proposed = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !int.TryParse(proposed, out var value) || value <= 0;
        }
    }
}
