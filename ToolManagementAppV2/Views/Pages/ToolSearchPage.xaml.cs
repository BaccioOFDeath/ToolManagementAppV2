using System.Windows;
using System.Windows.Controls;

namespace ToolManagementAppV2.Views.Pages
{
    public partial class ToolSearchPage : Page
    {
        public ToolSearchPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => UpdateState();

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateState();

        private void UpdateState()
        {
            string state = ActualWidth < 800 ? "Narrow" : "Wide";
            VisualStateManager.GoToState(this, state, true);
        }
    }
}
