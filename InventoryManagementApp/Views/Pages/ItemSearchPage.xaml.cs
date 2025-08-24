using System.Windows;
using System.Windows.Controls;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ItemSearchPage : Page
    {
        public ItemSearchPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateState();
            if (DataContext is ItemManagementViewModel vm)
                await vm.SearchCommand.ExecuteAsync(null);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateState();

        private void UpdateState()
        {
            string state = ActualWidth < 800 ? "Narrow" : "Wide";
            VisualStateManager.GoToState(this, state, true);
        }
    }
}
