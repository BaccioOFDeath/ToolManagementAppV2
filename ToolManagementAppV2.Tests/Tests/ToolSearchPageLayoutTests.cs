using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class ToolSearchPageLayoutTests
    {
        [Fact]
        public void ToolLists_UseHorizontalVirtualizingStackPanel()
        {
            var page = new ToolSearchPage();

            var handPanel = page.HandToolsList.ItemsPanel.LoadContent();
            var handStack = Assert.IsType<VirtualizingStackPanel>(handPanel);
            Assert.Equal(Orientation.Horizontal, handStack.Orientation);
            Assert.Equal(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(page.HandToolsList));
            Assert.Equal(ScrollBarVisibility.Disabled, ScrollViewer.GetVerticalScrollBarVisibility(page.HandToolsList));
            Assert.True(VirtualizingStackPanel.GetIsVirtualizing(page.HandToolsList));

            var powerPanel = page.PowerToolsList.ItemsPanel.LoadContent();
            var powerStack = Assert.IsType<VirtualizingStackPanel>(powerPanel);
            Assert.Equal(Orientation.Horizontal, powerStack.Orientation);
            Assert.Equal(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(page.PowerToolsList));
            Assert.Equal(ScrollBarVisibility.Disabled, ScrollViewer.GetVerticalScrollBarVisibility(page.PowerToolsList));
            Assert.True(VirtualizingStackPanel.GetIsVirtualizing(page.PowerToolsList));
        }

        [Fact]
        public void HandToolsList_VirtualizesLargeCollections()
        {
            var page = new ToolSearchPage();
            page.HandToolsList.ItemsSource = Enumerable.Range(0, 1000)
                .Select(i => new Tool { ToolID = i.ToString(), NameDescription = $"Tool {i}" })
                .ToList();

            page.HandToolsList.Measure(new Size(800, 300));
            page.HandToolsList.Arrange(new Rect(0, 0, 800, 300));
            page.HandToolsList.UpdateLayout();

            Assert.NotNull(page.HandToolsList.ItemContainerGenerator.ContainerFromIndex(0));
            Assert.Null(page.HandToolsList.ItemContainerGenerator.ContainerFromIndex(999));
        }
    }
}
