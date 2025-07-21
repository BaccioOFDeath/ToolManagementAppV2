using System.Windows;
using System.Windows.Controls;
using ToolManagementAppV2;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class SearchResultsTileTemplateTests
    {
        [Fact]
        public void SearchResultsList_UsesTileTemplate()
        {
            var window = new MainWindow();
            var template = Assert.IsType<DataTemplate>(window.Resources["ToolTileTemplate"]);
            Assert.Same(template, window.SearchResultsList.ItemTemplate);
            var panel = window.SearchResultsList.ItemsPanel.LoadContent();
            var stackPanel = Assert.IsType<VirtualizingStackPanel>(panel);
            Assert.Equal(Orientation.Horizontal, stackPanel.Orientation);
            Assert.Equal(ScrollBarVisibility.Disabled, ScrollViewer.GetVerticalScrollBarVisibility(window.SearchResultsList));
            Assert.Equal(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(window.SearchResultsList));
        }
    }
}
