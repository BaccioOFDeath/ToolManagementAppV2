// Views/ItemDetailsWindow.xaml.cs
using System.Windows;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Extensions;

namespace ToolManagementAppV2.Views.Windows
{
    public partial class ItemDetailsWindow : Window
    {
        public ItemDetailsWindow(ItemModel item)
        {
            InitializeComponent();
            DataContext = new ItemDetailsViewModel(item, () => Close());
            this.DisposeDataContextOnUnload();
        }
    }
}
