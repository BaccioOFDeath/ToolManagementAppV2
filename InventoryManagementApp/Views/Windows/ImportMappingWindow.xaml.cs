// File: Views/ImportMappingWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
    }
}