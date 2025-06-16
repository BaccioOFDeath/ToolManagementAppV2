using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using ToolManagementAppV2;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class MainWindowTabBindingTests
    {
        [StaFact]
        public void SwitchingTabs_DoesNotBreakToolsListBinding()
        {
            var window = new MainWindow();
            var vm = Assert.IsType<MainViewModel>(window.DataContext);

            // ToolsList should be bound to the Tools collection
            Assert.True(BindingOperations.IsDataBound(window.ToolsList, ItemsControl.ItemsSourceProperty));
            Assert.Same(vm.Tools, window.ToolsList.ItemsSource);

            var tabControl = window.MyTabControl;
            var searchTab = tabControl.Items.OfType<TabItem>().First(t => t.Header!.ToString() == "Search Tools");
            var toolsTab = tabControl.Items.OfType<TabItem>().First(t => t.Header!.ToString() == "Tool Management");

            tabControl.SelectedItem = searchTab;
            window.MyTabControl_SelectionChanged(tabControl, new SelectionChangedEventArgs(TabControl.SelectionChangedEvent, null, null));

            tabControl.SelectedItem = toolsTab;
            window.MyTabControl_SelectionChanged(tabControl, new SelectionChangedEventArgs(TabControl.SelectionChangedEvent, null, null));

            Assert.True(BindingOperations.IsDataBound(window.ToolsList, ItemsControl.ItemsSourceProperty));
            Assert.Same(vm.Tools, window.ToolsList.ItemsSource);
        }
    }
}
