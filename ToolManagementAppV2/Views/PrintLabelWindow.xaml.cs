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
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2.Views
{
    /// <summary>
    /// Interaction logic for PrintLabelWindow.xaml
    /// </summary>
    public partial class PrintLabelWindow : Window
    {
        public PrintLabelWindow()
        {
            InitializeComponent();
            DataContext = new PrintLabelViewModel(() => Close());
        }
    }
}
