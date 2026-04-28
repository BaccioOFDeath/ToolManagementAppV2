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
            {
                vm.SelectedCategory = "All";
                await vm.SearchCommand.ExecuteAsync(null);
            }
        }

        private void UpdateState()
        {
            VisualStateManager.GoToState(this, "Wide", true);
        }
    }
}
