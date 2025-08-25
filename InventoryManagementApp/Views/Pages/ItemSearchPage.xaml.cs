using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ItemSearchPage : Page
    {
        public ItemSearchPage()
        {
            InitializeComponent();
            ItemsList.AddHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(ItemsList_OnPreviewMouseWheel), true);
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

        private void ItemsList_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                var scrollViewer = FindScrollViewer(d);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                    e.Handled = true;
                }
            }
        }

        private static ScrollViewer? FindScrollViewer(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is ScrollViewer sv)
                    return sv;
                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
