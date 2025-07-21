using Xunit;
using System.Windows.Controls;
using System.Linq;

namespace ToolManagementAppV2.Tests.Tests
{
    public class SearchTabPreloadTests
    {
        [Fact]
        public void SearchResultsList_LoadsWhenTabOpened()
        {
            var window = new ToolManagementAppV2.MainWindow();
            Assert.False(window.SearchResultsList.IsLoaded);

            var searchTab = window.MyTabControl.Items.OfType<TabItem>().First(t => t.Header!.ToString() == "Tool Search");
            window.MyTabControl.SelectedItem = searchTab;
            window.MyTabControl_SelectionChanged(window.MyTabControl, new SelectionChangedEventArgs(TabControl.SelectionChangedEvent, null, null));

            Assert.True(window.SearchResultsList.IsLoaded);
        }
    }
}
