using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Image = System.Windows.Controls.Image;
using Application = System.Windows.Application;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ManageItemsPage : Page
    {
        public ManageItemsPage()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).Host.Services.GetRequiredService<ItemsViewModel>();
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                if (!row.IsSelected)
                {
                    row.IsSelected = true;
                }
                e.Handled = true;
            }
        }

        private void DataGridRow_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.DataContextChanged += DataGridRow_DataContextChanged;
            }
        }

        private void DataGridRow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.DataContextChanged -= DataGridRow_DataContextChanged;
                ReleaseRowImage(row);
            }
        }

        private void DataGridRow_DataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                ReleaseRowImage(row);
            }
        }

        private static void ReleaseRowImage(DataGridRow row)
        {
            var img = FindVisualChild<Image>(row);
            if (img?.Source is BitmapImage bmp)
            {
                bmp.StreamSource?.Dispose();
            }
            if (img != null)
                img.Source = null;
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;
                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }
    }
}
