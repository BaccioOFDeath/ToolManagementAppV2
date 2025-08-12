using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ToolManagementAppV2.Services.Devices;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
{
    /// <summary>
    /// Interaction logic for ScannerStatusWindow.xaml
    /// </summary>
    public partial class ScannerStatusWindow : Window
    {
        public ScannerStatusWindow()
        {
            InitializeComponent();
            DataContext = new ScannerStatusViewModel(new ScannerService());
        }
    }
}
