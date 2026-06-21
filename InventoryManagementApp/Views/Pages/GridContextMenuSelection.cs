using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace InventoryManagementApp.Views.Pages
{
    internal static class GridContextMenuSelection
    {
        public static DataGridRow? SelectRow(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow ?? FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row == null)
                return null;

            row.IsSelected = true;
            row.Focus();

            if (FindAncestor<DataGrid>(row) is DataGrid grid)
                grid.SelectedItem = row.Item;

            return row;
        }

        public static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T match)
                    return match;

                current = GetParent(current);
            }

            return null;
        }

        private static DependencyObject? GetParent(DependencyObject current)
        {
            try
            {
                return VisualTreeHelper.GetParent(current)
                    ?? LogicalTreeHelper.GetParent(current);
            }
            catch (InvalidOperationException)
            {
                return LogicalTreeHelper.GetParent(current);
            }
        }
    }
}
