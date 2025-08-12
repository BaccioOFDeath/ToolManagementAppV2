// File: Views/ImportMappingWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
{
    public partial class ImportMappingWindow : Window
    {
        public ImportMappingWindow(IEnumerable<string> headers, IEnumerable<string> propertyNames)
        {
            InitializeComponent();
            DataContext = new ImportMappingViewModel(
                headers,
                propertyNames,
                () => { DialogResult = true; Close(); },
                () => { DialogResult = false; Close(); });
        }

        public ImportMappingViewModel VM => (ImportMappingViewModel)DataContext;
    }
}
