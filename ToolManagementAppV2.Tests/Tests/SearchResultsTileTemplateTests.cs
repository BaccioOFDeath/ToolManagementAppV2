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
            Assert.IsType<WrapPanel>(panel);
        }
    }
}
