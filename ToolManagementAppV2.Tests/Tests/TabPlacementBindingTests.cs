using System.Windows.Controls;
using ToolManagementAppV2;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class TabPlacementBindingTests
    {
        [Fact]
        public void ChangingTabPlacement_UpdatesTabStripPlacement()
        {
            var window = new MainWindow();
            var vm = Assert.IsType<MainViewModel>(window.DataContext);

            vm.TabPlacement = Dock.Right;

            Assert.Equal(Dock.Right, window.MyTabControl.TabStripPlacement);
            Assert.Equal(Dock.Right, DockPanel.GetDock(window.TabPlacementThumb));
        }
    }
}
