// File: Views/ImportMappingWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ToolManagementAppV2.ViewModels.ImportExport;

namespace ToolManagementAppV2.Views
{
    public partial class ImportMappingWindow : Window
    {
        public ImportMappingWindow(IEnumerable<string> headers, IEnumerable<string> propertyNames)
        {
            InitializeComponent();
            DataContext = new ImportMappingViewModel(headers, propertyNames);
        }

        public ImportMappingViewModel VM => (ImportMappingViewModel)DataContext;

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (VM.Mappings.Any(m => string.IsNullOrEmpty(m.SelectedColumn)))
                return;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
