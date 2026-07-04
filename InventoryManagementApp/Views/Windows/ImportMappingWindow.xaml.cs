// File: Views/ImportMappingWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class ImportMappingWindow : Window
    {
        public ImportMappingWindow(IEnumerable<string> headers, IEnumerable<string> propertyNames, IEnumerable<string>? requiredPropertyNames = null)
        {
            InitializeComponent();
            this.UseResponsiveDefaultSize(980, 700);
            DataContext = new ImportMappingViewModel(
                headers,
                propertyNames,
                () => { DialogResult = true; Close(); },
                () => { DialogResult = false; Close(); },
                requiredPropertyNames);
            this.DisposeDataContextOnUnload();
        }

        public ImportMappingViewModel VM => (ImportMappingViewModel)DataContext;

        private void MappingComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ComboBox { IsDropDownOpen: false } comboBox)
                return;

            e.Handled = true;

            var parentGrid = FindAncestor<DataGrid>(comboBox);
            parentGrid?.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = comboBox
            });
        }

        private static T? FindAncestor<T>(DependencyObject? current)
            where T : DependencyObject
        {
            while (current is not null)
            {
                current = VisualTreeHelper.GetParent(current);
                if (current is T ancestor)
                    return ancestor;
            }

            return null;
        }
    }
}
