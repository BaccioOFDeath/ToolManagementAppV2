using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class ToolSearchPageLayoutTests
    {
        [Fact]
        public void ToolsList_UsesHorizontalVirtualizingStackPanel()
        {
            var page = new ToolSearchPage();

            var panel = page.ToolsList.ItemsPanel.LoadContent();
            var stack = Assert.IsType<VirtualizingStackPanel>(panel);
            Assert.Equal(Orientation.Horizontal, stack.Orientation);
            Assert.Equal(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(page.ToolsList));
            Assert.Equal(ScrollBarVisibility.Disabled, ScrollViewer.GetVerticalScrollBarVisibility(page.ToolsList));
            Assert.True(VirtualizingStackPanel.GetIsVirtualizing(page.ToolsList));
            Assert.IsType<Border>(page.ToolsList.ItemTemplate.LoadContent());
        }

        [Fact]
        public void ToolsList_VirtualizesLargeCollections()
        {
            var page = new ToolSearchPage();
            page.ToolsList.ItemsSource = Enumerable.Range(0, 1000)
                .Select(i => new Tool { ToolID = i, NameDescription = $"Tool {i}" })
                .ToList();

            page.ToolsList.Measure(new Size(800, 300));
            page.ToolsList.Arrange(new Rect(0, 0, 800, 300));
            page.ToolsList.UpdateLayout();

            Assert.NotNull(page.ToolsList.ItemContainerGenerator.ContainerFromIndex(0));
            Assert.Null(page.ToolsList.ItemContainerGenerator.ContainerFromIndex(999));

            var first = (FrameworkElement)page.ToolsList.ItemContainerGenerator.ContainerFromIndex(0);
            Assert.True(first.ActualWidth > 0 && first.ActualHeight > 0);
        }

        [Fact]
        public void SearchButton_BoundToSearchCommand()
        {
            var executed = false;
            var vm = new TestVm(() => executed = true);
            var page = new ToolSearchPage { DataContext = vm };

            var root = (Grid)page.Content;
            var border = (Border)root.Children[0];
            var innerGrid = (Grid)border.Child;
            var button = Assert.IsType<Button>(innerGrid.Children[2]);

            Assert.Same(vm.SearchCommand, button.Command);
            button.Command.Execute(null);
            Assert.True(executed);
        }

        class TestVm
        {
            public IRelayCommand SearchCommand { get; }
            public TestVm(Action onExecute) => SearchCommand = new RelayCommand(onExecute);
        }
    }
}
